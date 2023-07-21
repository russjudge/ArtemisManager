using AMCommunicator;
using ArtemisManagerAction;
using Lnk;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {

        public static Main? MainWindow { get; set; } = null;
        public static bool MustExit { get; set; }

        public static event EventHandler<StatusUpdateEventArgs>? StatusUpdate;
        private static void RaiseStatusUpdate(string message, params object[] parameters)
        {
            StatusUpdate?.Invoke(null, new StatusUpdateEventArgs(message, parameters));
        }
        public static event EventHandler<StatusUpdateEventArgs>? PopupEvent;
        private static void RaisePopupEvent(string message, params object[] parameters)
        {
            PopupEvent?.Invoke(null, new StatusUpdateEventArgs(message, parameters));
        }

        public static readonly string StartupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
        public static void FulfillModPackageRequest(IPAddress? requestSource, string itemRequestedIdentifier, ModItem? mod)
        {
            if (requestSource != null)
            {
                if (System.IO.File.Exists(Path.Combine(ModManager.ModArchiveFolder, itemRequestedIdentifier)))
                {
                    List<byte> data = new();
                    byte[] buffer = new byte[32768];
                    int bytesRead;
                    using (FileStream fs = new(Path.Combine(ModManager.ModArchiveFolder, itemRequestedIdentifier), FileMode.Open))
                    {
                        while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            byte[] buffer2 = new byte[bytesRead];
                            Array.Copy(buffer, 0, buffer2, 0, bytesRead);
                            data.AddRange(buffer2);
                        }
                    }
                    Network.Current?.SendItem(requestSource, data.ToArray(), mod?.GetJSON());
                }
            }
        }
        public static void ChangeSetting(string settingName,string value)
        {
            switch(settingName)
            {
                case "ConnectOnStart":
                    Properties.Settings.Default.ConnectOnStart = bool.Parse(value);
                    break;
                case "ListeningPort":
                    Properties.Settings.Default.ListeningPort = int.Parse(value);
                    break;
                case "IsMaster":
                    break;
            }
            Properties.Settings.Default.Save();
        }
        public static bool ProcessPCAction(PCActions action, bool force, IPAddress? source)
        {
            if (!force)
            {
                if (MessageBox.Show(
                    "The follow action is being requested: " 
                    + action.ToString()
                    + ".\r\nDo you wish to allow this?",
                    "Action requested", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return false;
                }
            }

            bool WasProcessed;
            switch (action)
            {
                
                case PCActions.CloseApp:
                    //Handled elsewhere.
                    WasProcessed = true;
                    break;
                case PCActions.RestartPC:
                    ProcessStartInfo startInfo = new("shutdown", "/g /t 0 /f");
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    System.Diagnostics.Process.Start(startInfo);
                    WasProcessed = true;
                    break;
                case PCActions.CheckForUpdate:
                    Task.Run(async () =>
                    {
                        var result = await UpdateCheck(false, source);

                        if (result.Item1)
                        {
                            var result2 = await DoUpdate(false, result.Item2);
                            if (result2)
                            {
                                if (source != null)
                                {
                                    Network.Current?.SendAlert(source, AMCommunicator.Messages.AlertItems.UpdateCheckSuccess, "Starting update");
                                }
                                Environment.Exit(0);
                            }
                        }
                    });
                    
                    WasProcessed = true;
                    break;
                case PCActions.ShutdownPC:
                    ProcessStartInfo startInfo2 = new("shutdown", "/sg /t 0 /f");
                    startInfo2.UseShellExecute = false;
                    startInfo2.CreateNoWindow = true;
                    System.Diagnostics.Process.Start(startInfo2);
                    
                    WasProcessed = true;
                    break;
                case PCActions.SendClientInformation:
                    var mods = ArtemisManagerAction.ArtemisManager.GetInstalledMods();

                    var jsonMods = new List<string>();

                    foreach (var mod in mods)
                    {
                        jsonMods.Add(mod.GetJSON());
                    }
                    bool AretmisIsRunning = ArtemisManagerAction.ArtemisManager.IsArtemisRunning();
                    bool IsRunningUnderMyControl = ArtemisManagerAction.ArtemisManager.IsRunningArtemisUnderMyControl();
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    Network.Current.SendClientInfo(
                        source, Properties.Settings.Default.IsAMaster,
                        Properties.Settings.Default.ConnectOnStart, jsonMods.ToArray(),
                        System.Array.Empty<string>(),
                        AretmisIsRunning,
                        IsRunningUnderMyControl,
                        IsThisAppInStartup());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
                    WasProcessed = true;
                    break;
                case PCActions.AddAppToStartup:
                    CreateShortcutInStartup();
                    WasProcessed = true;
                    break;
                case PCActions.RemoveAppFromStartup:
                    RemoveShortcutFromStartup();
                    WasProcessed = true;
                    break;
                
                default:
                    WasProcessed = false;
                    break;

            }
            return WasProcessed;
        }
        public static string GetAppVersion()
        {
            string retVal = string.Empty;
            var assm = System.Reflection.Assembly.GetEntryAssembly();
            if (assm != null)
            {
                var nm = assm.GetName();
                if (nm != null && nm.Version != null)
                {
                    retVal = nm.Version.ToString();
                }
            }
            return retVal;
        }
        const string UpdateSoftwareSourceURL = "https://artemis.russjudge.com/software";
        const string UpdateVersionDataURL = UpdateSoftwareSourceURL + "/artemismanager.version";
        const string UpdateSetupFileURL = UpdateSoftwareSourceURL + "/artemismanager.msi";
        /// <summary>
        /// Determines whether or not an update is available.
        /// </summary>
        /// <param name="AlertIfCannotCheck">send "true" to send an alert to the source that it could not access the website of the update to check.  Possible: have update transmitted from source.</param>
        /// <returns></returns>
        public async static Task<Tuple<bool, string>> UpdateCheck(bool AlertIfCannotCheck, IPAddress? source = null)
        {
            bool UpdateNeeded = false;
            string setupFile = UpdateSetupFileURL;
            string[] info;
            try
            {
                using (HttpClient client = new())
                {
                    var data = await client.GetStringAsync(UpdateVersionDataURL);
                    //line 1 = version
                    //line 2 = setup file to download.
                    info = data.Replace("\r", string.Empty).Split('\n');
                    if (info.Length > 1)
                    {
                        UpdateNeeded = (info[0] != GetAppVersion());
                    }
                    setupFile = UpdateSoftwareSourceURL + "/" + info[1];
                }
                RaiseStatusUpdate("Update check complete. {0}", UpdateNeeded ? "Updated needed." : "No Update needed.");
            }
            catch (Exception ex)
            {
                RaiseStatusUpdate("Unable to check for update: {0}", ex.Message);
                if (AlertIfCannotCheck)
                {
                    MessageBox.Show("Unable to check for update: \r\n" + ex.Message, "Update Check Failed");
                    
                }
                else
                {
                    if (source != null)
                    {
                        Network.Current?.SendAlert(source, AMCommunicator.Messages.AlertItems.UpdateCheckFail, "Unable to check for update: " + ex.Message);
                    }
                }
                RaiseStatusUpdate("Update check failed: {0}", ex.Message);
            }
            //russjudge.com/software/artemismanager.version
            //russjudge.com/software/artemismanager.msi

            return new Tuple<bool, string>(UpdateNeeded, setupFile);
        }
        public static async Task<bool> DoUpdate(bool doPrompt, string setupURL)
        {
            //1. Download the msi file.
            //2. Start the setup and exit.
            if (doPrompt)
            {
                if (MessageBox.Show("An update to Artemis Manager was found.\r\nDo you wish to download the update?", "Update Found", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
            try
            {
                string installFile = Path.GetTempFileName() + ".msi";
                using (HttpClient client = new())
                {
                    using (Stream strm = await client.GetStreamAsync(setupURL))
                    {
                        int bytesRead = 0;
                        byte[] buffer = new byte[32768];
                        
                        using (FileStream fs = new FileStream(installFile, FileMode.Create))
                        {
                            while ((bytesRead = strm.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
                RaiseStatusUpdate("Starting update.");
                System.Diagnostics.ProcessStartInfo startInfo = new ProcessStartInfo(installFile);
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {

                RaiseStatusUpdate("Unable to perform update: " + ex.Message);
            }
            return true;
        }

        public static bool IsThisAppInStartup()
        {
            bool retVal = false;
            var asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm != null)
            {
                string appLocation = asm.Location;
                FileInfo fle = new(appLocation);
                string target = Path.Combine(StartupFolder, fle.Name.Replace(".dll", string.Empty) + ".lnk");

                if (File.Exists(target))
                {
                    var data = Lnk.Lnk.LoadFile(target);
                    if (data != null)
                    {
                        try
                        {
                            retVal = data.TargetIDs[data.TargetIDs.Count - 1].Value.Contains(fle.Name.Replace(".dll", ".exe"), StringComparison.InvariantCultureIgnoreCase);
                        }
                        catch (NullReferenceException)
                        {
                            retVal = false;
                        }
                    }
                }
            }
            return retVal;
        }
        [SupportedOSPlatform("windows")]
        public static void RemoveWindowsShortcutFromStartup()
        {
            var assm = System.Reflection.Assembly.GetEntryAssembly();
            if (assm != null)
            {
                string loc = assm.Location.Replace(".dll", ".exe");
                foreach (var l in new DirectoryInfo(StartupFolder).GetFiles("*.lnk"))
                {
                    var lnkFile = Lnk.Lnk.LoadFile(l.FullName);
                    if (lnkFile.TargetIDs[lnkFile.TargetIDs.Count - 1].Value.Contains(new FileInfo(loc).Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        l.Delete();
                        break;
                    }
                }
            }
        }
        [SupportedOSPlatform("linux")]
        public static void RemoveLinuxShortcutFromStartup()
        {
            var assm = System.Reflection.Assembly.GetEntryAssembly();
            if (assm != null)
            {
                string target = Path.Combine(StartupFolder, "ArtemisManager");
                if (File.Exists(target))
                {
                    File.Delete(target);
                }
            }
        }
        public static void RemoveShortcutFromStartup()
        {
            if (OperatingSystem.IsWindows())
            {
                RemoveWindowsShortcutFromStartup();
            }
            else if (OperatingSystem.IsLinux())
            {

            }
        }
        [SupportedOSPlatform("linux")]
        public static void CreateLinuxShortcutInStartup()
        {
            var asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm != null)
            {
                string target = Path.Combine(StartupFolder, "ArtemisManager");
                using (StreamWriter sw = new (target))
                {
                    sw.WriteLine("#!/usr/bin");
                    sw.WriteLine(asm.Location.Replace(".dll", ".exe") + " FROMSTARTUPFOLDER");
                }
                Process.Start("chmod 777 " + target);
            }
        }
        public static void CreateShortcutInStartup()
        {
            if (OperatingSystem.IsWindows())
            {
                CreateWindowsShortcutInStartup();
            }
            else if (OperatingSystem.IsLinux())
            {
                CreateLinuxShortcutInStartup();
            }
        }
        [SupportedOSPlatform("windows")]
        public static void CreateWindowsShortcutInStartup()
        {
            //var asm = System.Reflection.Assembly.GetEntryAssembly();
            //if (asm != null)
            //{
                //string appLocation = asm.Location.Replace(".dll", ".exe");
               // Abraham.Windows.Shell.AutostartFolder.AddShortcut(appLocation, null, "Artemis Manager", string.Empty);
                
            var temp = Path.GetTempFileName() + ".vbs";
            var asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm != null)
            {
                string appLocation = asm.Location;
                FileInfo fle = new(appLocation);
                string target = Path.Combine(StartupFolder, fle.Name.Replace(".dll", string.Empty) + ".lnk");
                string commandLine = "cscript";
                using (StreamWriter sw = new(temp))
                {
                    sw.WriteLine("Set oWS = WScript.CreateObject(\"WScript.Shell\")");
                    
                    sw.WriteLine("sLinkFile = \"" + target + "\"");
                    sw.WriteLine("Set oLink = oWS.CreateShortcut(sLinkFile)");
                    sw.WriteLine("oLink.TargetPath = \"" + appLocation.Replace(".dll", ".exe") + " FROMSTARTUPFOLDER\"");
                    sw.WriteLine("oLink.IconLocation = \"" + appLocation.Replace(".dll", ".exe") + ", \"");
                    sw.WriteLine("oLink.Description = \"Artemis Manager\"");
                    sw.WriteLine("oLink.Save");
                    //sw.WriteLine("objFSO.DeleteFile(\"" + temp + "\")");
                }
                var startInfo = new ProcessStartInfo(commandLine, temp)
                {
                    ErrorDialog = true
                };

                Process process = new()
                {
                    StartInfo = startInfo
                };
                process.Start();

                process.WaitForExit();
                File.Delete(temp);
                    
                                /*
                 * Set oWS = WScript.CreateObject("WScript.Shell")
sLinkFile = "C:\MyShortcut.LNK"
Set oLink = oWS.CreateShortcut(sLinkFile)
    oLink.TargetPath = "C:\Program Files\MyApp\MyProgram.EXE"
 '  oLink.Arguments = ""
 '  oLink.Description = "MyProgram"   
 '  oLink.HotKey = "ALT+CTRL+F"
 '  oLink.IconLocation = "C:\Program Files\MyApp\MyProgram.EXE, 2"
 '  oLink.WindowStyle = "1"   
 '  oLink.WorkingDirectory = "C:\Program Files\MyApp"
oLink.Save
                 * */
            }
        }
    }
}
