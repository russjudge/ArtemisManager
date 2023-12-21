using AMCommunicator;
using ArtemisManagerAction;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisManagerUI
{
    public static class TakeAction
    {
        public const int SelfElement = 0;
        public const int AllConnectionsElement = 1;

        public static readonly IPAddress SelfIP = IPAddress.Loopback;
        public static readonly IPAddress AllConnections = IPAddress.Any;
        public static Main? MainWindow { get; set; } = null;
        public static bool MustExit { get; set; }

        public static event EventHandler<StatusUpdateEventArgs>? StatusUpdate;
        private static void RaiseStatusUpdate(string message, params object[] parameters)
        {
            StatusUpdate?.Invoke(null, new StatusUpdateEventArgs(message, parameters));
        }
        public static event EventHandler<StatusUpdateEventArgs>? PopupEvent;

        public static PCItem? SourcePC { get; set; }
        public static ObservableCollection<PCItem>? ConnectedPCs { get; set; }
        public static event EventHandler<ConnectionEventArgs>? ConnectionAdded;
        public static event EventHandler<ConnectionEventArgs>? ConnectionRemoved;
        public static PCItem ThisMachine { get; private set; }
        static TakeAction()
        {
            ConnectedPCs = [];
            ThisMachine = new PCItem(Dns.GetHostName() + " (Local)", IPAddress.Loopback)
            {
                AppVersion = GetAppVersion(),
                IsMaster = SettingsAction.Current.IsMaster,
                AppInStartFolder = IsThisAppInStartup(),
                InstalledMissions = new(ArtemisManager.GetInstalledMissions()),
                InstalledMods = new(ArtemisManager.GetInstalledMods()),
                ArtemisIsRunning = ArtemisManager.IsArtemisRunning(),

                Drives = new(DriveData.GetDriveData()),

                ConnectOnstart = SettingsAction.Current.ConnectOnStart,
                IsMainScreenServer = ArtemisManager.GetIsMainScreenServer(),
                IsRemote = false,
                IsUsingThisAppControlledArtemis = ArtemisManager.IsRunningArtemisUnderMyControl()
            };


            AddConnection(ThisMachine);
            TakeAction.SourcePC = ThisMachine;
            if (SettingsAction.Current.IsMaster)
            {
                AddConnection(new PCItem("All Connections", IPAddress.Any));
            };
        }
        public static void AddConnection(PCItem item)
        {
            ConnectedPCs?.Add(item);
            ConnectionAdded?.Invoke(null, new ConnectionEventArgs(item));
            //SetAllConnectionsInfo();

        }
        public static void RemoveConnection(PCItem item)
        {
            ConnectedPCs?.Remove(item);
            ConnectionRemoved?.Invoke(null, new ConnectionEventArgs(item));
            SetAllConnectionsInfo();
            //SetMainScreenServerIP();
        }

        public static readonly string StartupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup));

        public static void FulfillModPackageRequest(IPAddress? requestSource, string itemRequestedIdentifier, string json)
        {
            if (requestSource != null)
            {
                if (System.IO.File.Exists(Path.Combine(ModManager.ModArchiveFolder, itemRequestedIdentifier)))
                {
                    List<byte> data = [];
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

                    var dataArr = data.ToArray();
                    if (requestSource.ToString() == IPAddress.Any.ToString() && ConnectedPCs != null)
                    {
                        foreach (var pcItem in ConnectedPCs)
                        {
                            if (pcItem.IP != null)
                            {
                                if (pcItem.IP.ToString() != IPAddress.Any.ToString())
                                {
                                    Network.Current?.SendItem(pcItem.IP, dataArr, json);
                                }
                            }
                        }
                    }
                    else
                    {
                        Network.Current?.SendItem(requestSource, dataArr, json);
                    }

                }
            }
        }
        public static void FulfillModPackageRequest(IPAddress? requestSource, string itemRequestedIdentifier, ModItem? mod)
        {
            FulfillModPackageRequest(requestSource, itemRequestedIdentifier, mod?.GetJSON() ?? string.Empty);
        }
        public static bool ProcessPCAction(PCActions action, bool force, IPAddress? source)
        {
            if (!force)
            {
                if (MessageBox.Show(string.Format("The follow action is being requested: {0}.{1}Do you wish to allow this?", action.ToString(), Environment.NewLine),
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
                    ProcessStartInfo startInfo = new("shutdown", "/g /t 0 /f")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
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
                    ProcessStartInfo startInfo2 = new("shutdown", "/sg /t 0 /f")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    System.Diagnostics.Process.Start(startInfo2);

                    WasProcessed = true;
                    break;
                case PCActions.SendClientInformation:
                    SendClientInfo(source);
                    WasProcessed = true;
                    break;
                case PCActions.AddAppToStartup:
                    CreateShortcutInStartup();
                    ThisMachine.AppInStartFolder = true;
                    SendClientInfo(IPAddress.Any);
                    WasProcessed = true;


                    break;
                case PCActions.RemoveAppFromStartup:
                    RemoveShortcutFromStartup();
                    ThisMachine.AppInStartFolder = false;
                    SendClientInfo(IPAddress.Any);
                    WasProcessed = true;

                    break;
                case PCActions.SetAsMainScreenServer:
                    var ip = Network.GetMyIP();
                    if (ip == null)
                    {
                        ArtemisManager.SetServerIP(IPAddress.Loopback);

                    }
                    else
                    {
                        ArtemisManager.SetServerIP(ip);
                    }
                    ThisMachine.IsMainScreenServer = true;
                    SendClientInfo(IPAddress.Any);
                    WasProcessed = true;

                    break;
                case PCActions.RemoveAsMainScreenServer:
                    IPAddress? ipx = Network.GetMyIP();
                    string me = IPAddress.Loopback.ToString();
                    if (ipx != null)
                    {
                        me = ipx.ToString();
                    }
                    ipx = null;
                    if (ConnectedPCs != null)
                    {
                        foreach (var pc in ConnectedPCs)
                        {
                            if (pc.IsMainScreenServer && !TakeAction.IsBroadcast(pc.IP) && !TakeAction.IsLoopback(pc.IP) && pc.IP != null && pc.IP.ToString() != me)
                            {
                                ipx = pc.IP;
                                break;
                            }
                        }
                    }
                    ArtemisManager.SetServerIP(ipx);
                    ThisMachine.IsMainScreenServer = false;
                    SendClientInfo(IPAddress.Any);
                    WasProcessed = true;

                    break;

                default:
                    WasProcessed = false;
                    break;

            }
            return WasProcessed;
        }

        public static void SetMainScreenServerIP()
        {
            var ip = GetMainScreenServerIP();

            ArtemisManager.SetServerIP(ip);
            if (ip != null)
            {
                ThisMachine.IsMainScreenServer = (ip.Equals(Network.MyIP) || ip.Equals(IPAddress.Loopback));
            }
        }
        public static IPAddress? GetMainScreenServerIP()
        {
            IPAddress? ipx = Network.GetMyIP();

            if (ConnectedPCs != null)
            {
                foreach (var pc in ConnectedPCs)
                {
                    if (pc.IsMainScreenServer && !TakeAction.IsBroadcast(pc.IP) && pc.IP != null)
                    {
                        ipx = pc.IP;
                    }
                }
            }
            return ipx;
        }

        /// <summary>
        /// Gets list of all screen resolutions available for the current screen.
        /// NOTE: this MIGHT only work for the screen that the app is running on--or the primary screen (not sure).  This needs tested.
        /// </summary>
        /// <returns></returns>
        public static System.Drawing.Size[] GetAvailableScreenResolutions()
        {
            List<System.Drawing.Size> retVal = [];
            //(gwmi - N "root\wmi" - Class WmiMonitorListedSupportedSourceModes)[0].MonitorSourceModes | select { "$($_.HorizontalActivePixels)x$($_.VerticalActivePixels)"}
            try
            {
                using ManagementObjectSearcher searcher = new("root\\WMI", "SELECT * FROM WmiMonitorListedSupportedSourceModes");
                foreach (ManagementObject queryObj in searcher.Get().Cast<ManagementObject>())
                {
                    if (queryObj["MonitorSourceModes"] is Array monitorSourceModes && monitorSourceModes.Length > 0)
                    {
                        foreach (var mode in monitorSourceModes)
                        {
                            if (mode is ManagementBaseObject md)
                            {

                                //WmiMonitorSupportedVideoModes
                                //VideoModeDescriptor vmd = 
                                var horizontalPixels = Convert.ToInt32(md["HorizontalActivePixels"]);
                                var verticalPixels = Convert.ToInt32(md["VerticalActivePixels"]);
                                System.Drawing.Size sz = new(horizontalPixels, verticalPixels);
                                if (!retVal.Contains(sz))
                                {
                                    retVal.Add(sz);
                                }
                            }
                        }
                    }
                }
            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
                MessageBox.Show("(ManagmentException) There was an error getting available screen resolutions:" + Environment.NewLine + e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error getting available screen resolutions:" + Environment.NewLine + e.Message);
            }
            return [.. retVal];
        }

        public static void SendClientInfo(IPAddress? source)
        {
            if (source != null)
            {
                var mods = ArtemisManager.GetInstalledMods();
                var missions = ArtemisManager.GetInstalledMissions();
                var jsonMods = new List<string>();
                var jsonMissions = new List<string>();
                foreach (var mod in mods)
                {
                    jsonMods.Add(mod.GetJSON());
                }
                foreach (var mission in missions)
                {
                    jsonMissions.Add(mission.GetJSON());
                }
                bool AretmisIsRunning = ArtemisManagerAction.ArtemisManager.IsArtemisRunning();
                bool IsRunningUnderMyControl = ArtemisManagerAction.ArtemisManager.IsRunningArtemisUnderMyControl();
                var modsArray = jsonMods.ToArray();
                var missionsArray = jsonMissions.ToArray();
                bool InStartup = IsThisAppInStartup();
                bool isMainScreenServer = ArtemisManager.GetIsMainScreenServer();
                if (source.ToString() == IPAddress.Any.ToString() && ConnectedPCs != null)
                {
                    foreach (var client in ConnectedPCs)
                    {
                        if (client.IP != null)
                        {
                            Network.Current?.SendClientInfo(
                               client.IP, SettingsAction.Current.IsMaster,
                               SettingsAction.Current.ConnectOnStart, modsArray,
                               missionsArray,
                               AretmisIsRunning,
                               IsRunningUnderMyControl,
                               InStartup, isMainScreenServer);
                        }
                    }
                }
                else
                {
                    Network.Current?.SendClientInfo(
                        source, SettingsAction.Current.IsMaster,
                        SettingsAction.Current.ConnectOnStart, modsArray,
                        missionsArray,
                        AretmisIsRunning,
                        IsRunningUnderMyControl,
                        InStartup, isMainScreenServer);
                }
            }
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
                    MessageBox.Show("Unable to check for update: " + Environment.NewLine + ex.Message, "Update Check Failed");

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
                if (MessageBox.Show(string.Format("An update to Artemis Manager was found.{0}Do you wish to download the update?", Environment.NewLine),
                    "Update Found", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return false;
                }
            }
            try
            {
                string installFile = Path.GetTempFileName() + ".msi";
                using (HttpClient client = new())
                {
                    using Stream strm = await client.GetStreamAsync(setupURL);
                    int bytesRead = 0;
                    byte[] buffer = new byte[32768];

                    using FileStream fs = new(installFile, FileMode.Create);
                    while ((bytesRead = strm.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                    }
                }
                RaiseStatusUpdate("Starting update.");
                ProcessStartInfo startInfo = new(installFile)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
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
                            retVal = data.TargetIDs[^1].Value.Contains(fle.Name.Replace(".dll", ".exe"), StringComparison.InvariantCultureIgnoreCase);
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
                    if (lnkFile.TargetIDs[^1].Value.Contains(new FileInfo(loc).Name, StringComparison.InvariantCultureIgnoreCase))
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
                using (StreamWriter sw = new(target))
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
        public static void SaveEngineeringPreset(string filename, string JsonData)
        {
            PresetsFile? file = JsonSerializer.Deserialize<PresetsFile>(JsonData);
            if (file != null)
            {
                string target = Path.Combine(ArtemisManager.EngineeringPresetsFolder, filename);

                file.Save(target);
                Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfEngineeringPresets, string.Empty, ArtemisManager.GetEngineeringPresetFiles());
            }
        }
        public static void SaveArtemisINISettingsFile(string filename, string stringData)
        {
            string target = Path.Combine(ArtemisManager.ArtemisINIFolder, filename);
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            using StreamWriter sw = new(target);
            sw.WriteLine(stringData);
            Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
        }
        public static void SaveControlsINISettingsFile(string filename, string data)
        {
            string target = Path.Combine(ArtemisManager.ControlsINIFolder, filename);
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            using StreamWriter sw = new(target);
            sw.WriteLine(data);
            Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
        }
        public static void SaveDMXCommandsSettingsFile(string filename, string data)
        {
            string target = Path.Combine(ArtemisManager.DMXCommandsFolder, filename);
            if (File.Exists(target))
            {
                File.Delete(target);
            }
            using StreamWriter sw = new(target);
            sw.WriteLine(data);
            Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
        }
        public static bool DoChangePassword(string? password)
        {
            bool success = false;

            if (ConnectedPCs != null)
            {
                switch (MessageBox.Show(
                    string.Format("Update all connected peers with new password?{0}Warning--if not changing the password on all connected peers will mean these peers will NOT be able to reconnect if they lose connection.{0}The current connection will be unaffected.", Environment.NewLine), "Update Managers", MessageBoxButton.YesNoCancel))
                {
                    case MessageBoxResult.Yes:
                        foreach (var pc in ConnectedPCs)
                        {
                            if (pc.IP != null)
                            {
                                string pass = string.Empty;
                                if (!string.IsNullOrEmpty(password))
                                {
                                    pass = password;
                                }
                                Network.Current?.SendChangePassword(pc.IP, pass);
                            }
                        }
                        success = true;
                        break;
                    case MessageBoxResult.No:
                        success = true;
                        break;
                    case MessageBoxResult.Cancel:
                        success = false;
                        break;
                }
            }
            return success;
        }
        public static bool DoChangeSetting(string setting, string? value)
        {
            if (setting == "NetworkPassword" || setting == "Password")
            {
                return DoChangePassword(value);
            }
            else
            {
                bool success = true;

                return success;
            }
        }
        public static bool IsBroadcast(IPAddress? address)
        {
            return (address?.ToString() == AllConnections.ToString());
        }

        public static bool IsLoopback(IPAddress? address)
        {
            return (address?.ToString() == SelfIP.ToString());
        }

        public static void SetAllConnectionsInfo()
        {
            if (ConnectedPCs != null && ConnectedPCs.Count > AllConnectionsElement)
            {
                bool? allConnectOnStart = null;
                bool allConnectOnStartDecided = false;

                bool? allArtemisIsRunning = null;
                bool allArtemisIsRunningDecided = false;

                bool? allIsUsingThisApp = null;
                bool allIsUsingThisAppDecided = false;

                bool? allInStartFolder = null;
                bool allInStartFolderDecided = false;

                string allAppVersion = string.Empty;
                foreach (var item in ConnectedPCs)
                {
                    if (item.IP != null && !TakeAction.IsBroadcast(item.IP))
                    {
                        if (string.IsNullOrEmpty(allAppVersion))
                        {
                            allAppVersion = item.AppVersion;
                        }
                        else
                        {
                            if (allAppVersion != "Mixed" && allAppVersion != item.AppVersion)
                            {
                                allAppVersion = "Mixed";
                            }
                        }
                        if (!allConnectOnStartDecided)
                        {
                            if (allConnectOnStart == null)
                            {
                                allConnectOnStart = item.ConnectOnstart;
                            }
                            else
                            {
                                if (allConnectOnStart != item.ConnectOnstart)
                                {
                                    allConnectOnStart = null;
                                    allConnectOnStartDecided = true;
                                }
                            }
                        }
                        if (!allArtemisIsRunningDecided)
                        {
                            if (allArtemisIsRunning == null)
                            {
                                allArtemisIsRunning = item.ArtemisIsRunning;
                            }
                            else
                            {
                                if (allArtemisIsRunning != item.ArtemisIsRunning)
                                {
                                    allArtemisIsRunning = null;
                                    allArtemisIsRunningDecided = true;
                                }
                            }
                        }

                        if (!allIsUsingThisAppDecided)
                        {
                            if (allIsUsingThisApp == null)
                            {
                                allIsUsingThisApp = item.IsUsingThisAppControlledArtemis;
                            }
                            else
                            {
                                if (allIsUsingThisApp != item.IsUsingThisAppControlledArtemis)
                                {
                                    allIsUsingThisApp = null;
                                    allIsUsingThisAppDecided = true;
                                }
                            }
                        }
                        if (!allInStartFolderDecided)
                        {
                            if (allInStartFolder == null)
                            {
                                allInStartFolder = item.AppInStartFolder;
                            }
                            else
                            {
                                if (allInStartFolder != item.AppInStartFolder)
                                {
                                    allInStartFolder = null;
                                    allInStartFolderDecided = true;
                                }
                            }
                        }
                    }
                }
                ConnectedPCs[AllConnectionsElement].ConnectOnstart = allConnectOnStart;
                ConnectedPCs[AllConnectionsElement].ArtemisIsRunning = allArtemisIsRunning;
                ConnectedPCs[AllConnectionsElement].IsUsingThisAppControlledArtemis = allIsUsingThisApp;
                ConnectedPCs[AllConnectionsElement].AppInStartFolder = allInStartFolder;
                ConnectedPCs[AllConnectionsElement].AppVersion = allAppVersion;
                if (ConnectedPCs?.Count > 2)
                {
                    SetMainScreenServerIP();
                }
            }
        }
        private static void DoSendPing(IPAddress address)
        {
            if (TakeAction.IsBroadcast(address))
            {

            }
            else
            {
                if (TakeAction.IsLoopback(address))
                {
                    //Pinging self here--maybe we do something???
                }
                else
                {
                    Network.Current?.SendPing(address);
                }
            }
        }
        public static void SendPing(IPAddress? address)
        {
            if (address != null)
            {
                if (IsBroadcast(address))
                {
                    if (ConnectedPCs != null)
                    {
                        foreach (var connection in ConnectedPCs)
                        {
                            if (connection != null && connection.IP != null)
                            {
                                DoSendPing(connection.IP);
                            }
                        }
                    }
                }
                else
                {
                    DoSendPing(address);
                }
            }
        }
        private static void DoSendPCAction(IPAddress address, PCActions action)
        {
            if (TakeAction.IsBroadcast(address))
            {

            }
            else
            {
                if (TakeAction.IsLoopback(address))
                {
                    TakeAction.ProcessPCAction(action, true, address);
                }
                else
                {
                    Network.Current?.SendPCAction(address, action, true);
                }
            }
        }
        public static void SendPCAction(IPAddress? address, PCActions action)
        {
            if (address != null)
            {
                if (TakeAction.IsBroadcast(address))
                {
                    if (TakeAction.ConnectedPCs != null)
                    {
                        foreach (var connection in TakeAction.ConnectedPCs)
                        {
                            if (connection != null && connection.IP != null)
                            {
                                DoSendPCAction(connection.IP, action);
                            }
                        }
                    }
                }
                else
                {
                    DoSendPCAction(address, action);
                }
            }
        }
        private static void DoChangeSetting(IPAddress address, string setting, string value)
        {
            if (TakeAction.IsBroadcast(address))
            {

            }
            else
            {
                if (TakeAction.IsLoopback(address))
                {
                    SettingsAction.Current.SynchronizeEnabled = false;
                    SettingsAction.Current.ChangeSetting(setting, value);
                    SettingsAction.Current.Save();
                }
                else
                {
                    Network.Current?.SendChangeSetting(address, setting, value);
                }
            }
        }
        public static void SendChangeSetting(IPAddress? address, string setting, string? value)
        {
            if (address != null && value != null)
            {
                if (TakeAction.IsBroadcast(address))
                {
                    if (TakeAction.ConnectedPCs != null)
                    {
                        foreach (var connection in TakeAction.ConnectedPCs)
                        {
                            if (connection != null && connection.IP != null)
                            {

                                DoChangeSetting(connection.IP, setting, value);
                            }
                        }
                    }
                }
                else
                {
                    DoChangeSetting(address, setting, value);
                }
            }
        }

        private static void DoSendArtemisAction(IPAddress address, AMCommunicator.Messages.ArtemisActions action, Guid identifier, string JSON)
        {
            if (TakeAction.IsBroadcast(address))
            {

            }
            else
            {
                if (TakeAction.IsLoopback(address))
                {
                    TakeArtemisAction.ProcessArtemisAction(address, action, JSON);

                }
                else
                {
                    Network.Current?.SendArtemisAction(address, action, identifier, JSON);
                }
            }
        }
        public static void SendArtemisAction(IPAddress? address, AMCommunicator.Messages.ArtemisActions action, Guid identifier, string JSON)
        {
            if (address != null)
            {
                if (TakeAction.IsBroadcast(address))
                {
                    if (TakeAction.ConnectedPCs != null)
                    {
                        foreach (var connection in TakeAction.ConnectedPCs)
                        {
                            if (connection != null && connection.IP != null)
                            {

                                DoSendArtemisAction(connection.IP, action, identifier, JSON);
                            }
                        }
                    }
                }
                else
                {
                    DoSendArtemisAction(address, action, identifier, JSON);
                }
            }
        }
    }
}
