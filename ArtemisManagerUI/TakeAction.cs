using AMCommunicator;
using Lnk;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {
        public static readonly string StartupFolder = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "Start Menu", "Programs", "Startup");
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
                //if (MessageBox.Show("The follow action is being requested: " + action.ToString() + ".\r\nDo you wish to allow this?", "Action requested", MessageBoxButton.YesNo) == MessageBoxResult.No)
                //{
                //    return false;
                //}
            }

            bool WasProcessed;
            switch (action)
            {
                case ActionCommands.CloseApp:
                    //Handled elsewhere.
                    WasProcessed = true;
                    break;
                case ActionCommands.RestartPC:

                    System.Diagnostics.Process.Start("shutdown /r /t 0");
                    WasProcessed = true;
                    break;
                case ActionCommands.UpdateCheck:
                    //TODO: Add process to check for update.
                    if (UpdateCheck(true, source))
                    {
                        //TODO: do the update.  No prompt.
                    }
                    WasProcessed = true;
                    break;
                case ActionCommands.ShutdownPC:
                    System.Diagnostics.Process.Start("shutdown /s /t 0");
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
        public static bool UpdateCheck(bool AlertIfCannotCheck, IPAddress? source = null)
        {
            
            return false;
        }

        public static bool IsThisAppInStartup()
        {
            bool retVal = false;
            var asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm != null)
            {
                string appLocation = asm.Location;
                FileInfo fle = new FileInfo(appLocation);
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
        public static void RemoveShortcutFromStartup()
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
        public static void CreateShortcutInStartup()
        {
            var temp = Path.GetTempFileName() + ".vbs";
            var asm = System.Reflection.Assembly.GetEntryAssembly();
            if (asm != null)
            {
                string appLocation = asm.Location;
                FileInfo fle = new FileInfo(appLocation);
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
                var startInfo = new System.Diagnostics.ProcessStartInfo(commandLine, temp);
                startInfo.ErrorDialog = true;

                var process = new System.Diagnostics.Process();
                
                process.StartInfo = startInfo;
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
