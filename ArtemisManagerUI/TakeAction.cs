using AMCommunicator;
using Lnk;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {
        public static readonly string StartupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Windows", "Start Menu", "Programs", "Startup");
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
            
            bool WasProcessed = false;
            if (!force)
            {
                //if (MessageBox.Show("The follow action is being requested: " + action.ToString() + ".\r\nDo you wish to allow this?", "Action requested", MessageBoxButton.YesNo) == MessageBoxResult.No)
                //{
                //    return false;
                //}
            }
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

                    Network.Current.SendClientInfo(
                        source, Properties.Settings.Default.IsAMaster,
                        Properties.Settings.Default.ConnectOnStart, jsonMods.ToArray(),
                        System.Array.Empty<string>(), 
                        ArtemisManagerAction.ArtemisManager.IsArtemisRunning(),
                        ArtemisManagerAction.ArtemisManager.IsRunningArtemisUnderMyControl(),
                        IsThisAppInStartup());
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
            string targetLink = Path.Combine(StartupFolder, "Artemis.lnk");
            if (File.Exists(targetLink))
            {
                var data = Lnk.Lnk.LoadFile(targetLink);
                if (data != null)
                {
                    retVal = data.SourceFile.StartsWith(System.Reflection.Assembly.GetEntryAssembly().Location, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            return retVal;
        }
        public static void CreateShortcutInStartup()
        {
            var temp = Path.GetTempFileName() + ".vbs";
            string appLocation = System.Reflection.Assembly.GetEntryAssembly().Location;
            string appName = appLocation.Substring(0, appLocation.Length - 4);
            using (StreamWriter sw = new StreamWriter(temp))
            {
                sw.WriteLine("Set oWS = WScript.CreateObject(\"WScript.Shell\")");

                sw.WriteLine("sLinkFile = \"" + Path.Combine(StartupFolder, appName + ".lnk") + "\"");
                sw.WriteLine("Set oLink = oWS.CreateShortcut(sLinkFile)");
                sw.WriteLine("oLink.TargetPath = \"" + appLocation + "\"");
                sw.WriteLine("oLink.IconLocation = \"" + appLocation + ", 2\"");
                sw.WriteLine("oLink.Description = \"Artemis Manager\"");
                sw.WriteLine("oLink.Save");
                sw.WriteLine("objFSO.DeleteFile(\"" + temp + "\")");
            }
            System.Diagnostics.Process.Start("cscript " + temp);
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
