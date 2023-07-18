using AMCommunicator;
using ArtemisManagerAction;
using Lnk;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Documents;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {

        public static readonly string StartupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
        public static void FulfillModPackageRequest(IPAddress? requestSource, string itemRequestedIdentifier, ModItem? mod)
        {
            if (requestSource != null)
            {
                if (File.Exists(Path.Combine(ModManager.ModArchiveFolder, itemRequestedIdentifier)))
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
            }
            Properties.Settings.Default.Save();
        }
        public static bool ProcessPCAction(ActionCommands action, bool force, IPAddress? source)
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
                case ActionCommands.CloseApp:
                    //Handled elsewhere.
                    WasProcessed = true;
                    break;
                case ActionCommands.RestartPC:
                    ProcessStartInfo startInfo = new("shutdown", "/g /t 0 /f");
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    System.Diagnostics.Process.Start(startInfo);
                    WasProcessed = true;
                    break;
                case ActionCommands.UpdateCheck:
                    //TODO: Add process to check for update.
                    if (UpdateCheck(true))
                    {
                        //TODO: do the update.  No prompt.
                    }
                    WasProcessed = true;
                    break;
                case ActionCommands.ShutdownPC:
                    ProcessStartInfo startInfo2 = new("shutdown", "/sg /t 0 /f");
                    startInfo2.UseShellExecute = false;
                    startInfo2.CreateNoWindow = true;
                    System.Diagnostics.Process.Start(startInfo2);
                    
                    WasProcessed = true;
                    break;
                case ActionCommands.ClientInformationRequested:
                    var mods = ArtemisManagerAction.ArtemisManager.GetInstalledMods();

                    var jsonMods = new List<string>();

                    foreach (var mod in mods)
                    {
                        jsonMods.Add(mod.GetJSON());
                    }

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    Network.Current.SendClientInfo(
                        source, Properties.Settings.Default.IsAMaster,
                        Properties.Settings.Default.ConnectOnStart, jsonMods.ToArray(),
                        System.Array.Empty<string>(),
                        ArtemisManagerAction.ArtemisManager.IsArtemisRunning(),
                        ArtemisManagerAction.ArtemisManager.IsRunningArtemisUnderMyControl(),
                        IsThisAppInStartup());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
                    WasProcessed = true;
                    break;
                default:
                    WasProcessed = false;
                    break;

            }
            return WasProcessed;
        }
        /// <summary>
        /// Determines whether or not an update is available.
        /// </summary>
        /// <param name="AlertIfCannotCheck">send "true" to send an alert to the source that it could not access the website of the update to check.  Possible: have update transmitted from source.</param>
        /// <returns></returns>
        public static bool UpdateCheck(bool AlertIfCannotCheck)
        {
            //russjudge.com/software/artemismanager.version
            //russjudge.com/software/artemismanager.msi

            return false;
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
                        retVal = data.LocalPath.StartsWith(appLocation.Replace(".dll", ".exe"), StringComparison.InvariantCultureIgnoreCase);
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
                    if (lnkFile.LocalPath == loc)
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
                    sw.WriteLine(asm.Location.Replace(".dll", ".exe"));
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
                    sw.WriteLine("oLink.TargetPath = \"" + appLocation.Replace(".dll", ".exe") + "\"");
                    sw.WriteLine("oLink.IconLocation = \"" + appLocation + ", 2\"");
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
