using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArtemisManagerAction
{
    public class ArtemisManager
    {
        public const string Artemis = "Artemis";
        public static readonly Dictionary<string, Guid> ArtemisVersionIdentifiers = new();
        public const string RegistryArtemisInstallLocation = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\windows\\CurrentVersion\\App Paths\\" + ArtemisEXE;

        public const string SaveFileExtension = ".json";
        public const string INIFileExtension = ".ini";
        public const string XMLFileExtension = ".xml";
        public const string DATFileExtension = ".dat";
        public const string EXEFileExtension = ".exe";
        public const string TXTFileExtension = ".txt";

        public const string ArtemisUpgradesURLFolder = "https://artemis.russjudge.com/artemisupgrades/";
        public const string ExternalToolsURLFolder = "https://artemis.russjudge.com/Tools/";

        public static readonly string EngineeringPresetsFolder = Path.Combine(ModManager.DataFolder, "EngineeringPresets");
        public static readonly string ControlsINIFolder = Path.Combine(ModManager.DataFolder, "controlsINI");
        public static readonly string DMXCommandsFolder = Path.Combine(ModManager.DataFolder, "DMXCommands");
        public static readonly string ArtemisINIFolder = Path.Combine(ModManager.DataFolder, "artemisINI");
        public static readonly string ActiveLocallSettingsArtemisINIFileMarker = Path.Combine(ModManager.DataFolder, "ActiveLocalSettingsArtemisINI" + TXTFileExtension);


        public const string ArtemisDATSubfolder = "dat";
        
        public const string originalIdentifier = "Original";

        public const string ArtemisEngineeringFile = "engineeringSettings" + DATFileExtension;
        public const string OriginalArtemisEngineeringFile = originalIdentifier + DATFileExtension;

        

        public const string ArtemisEXE = Artemis + EXEFileExtension;
        public const string controlsINI = "controls" + INIFileExtension;
        public const string DMXCommands = "DMXcommands" + XMLFileExtension;
        public const string ArtemisINI = "artemis" + INIFileExtension;

        public const string ChangesTXT = "changes" + TXTFileExtension;
        static System.Diagnostics.Process? runningArtemisProcess = null;


        static ArtemisManager()
        {
            //Fixing Artemis versions to specific Guids.  Same thing will be done to mods eventually.
            ArtemisVersionIdentifiers.Add("2.0.0", new Guid("372902D8-CA57-46AA-9B17-488243C04D55"));
            ArtemisVersionIdentifiers.Add("2.1.1", new Guid("5814DE3D-B3D2-4D2E-AB71-A8634E0F9368"));
            ArtemisVersionIdentifiers.Add("2.4.0", new Guid("F46E14B7-A97B-4EAA-A060-A9034F7F55CB"));
            ArtemisVersionIdentifiers.Add("2.8.0", new Guid("D141B467-1A2F-48CE-8BDF-3540BDC48215"));
            ArtemisVersionIdentifiers.Add("2.8.1", new Guid("AF0EC3FE-D26A-4AAD-8E1A-8584DE044688"));  //If from zip file the exe file date should be 1/23/2022.
        }
        public static void SetActiveLocalArtemisINISettings(string file)
        {
            if (File.Exists(ActiveLocallSettingsArtemisINIFileMarker))
            {
                File.Delete(ActiveLocallSettingsArtemisINIFileMarker);
            }
            using (StreamWriter sw = new(ActiveLocallSettingsArtemisINIFileMarker))
            {
                sw.WriteLine(file);
            }
            ActivateLocalArtemisINISettings();
        }
        public static bool ActivateLocalArtemisINISettings()
        {
            bool retVal;
            if (!File.Exists(ActiveLocallSettingsArtemisINIFileMarker))
            {
                retVal = false;
            }
            else
            {
                string? source;
                using (StreamReader sr = new(ActiveLocallSettingsArtemisINIFileMarker))
                {
                    source = sr.ReadLine();
                }
                if (source == null)
                {
                    retVal = false;
                }
                else
                {
                    var local = new ArtemisINI(source);
                    string target = Path.Combine(ModItem.ActivatedFolder, ArtemisINI);
                    ArtemisINI? result = null;
                    if (File.Exists(target))
                    {
                        ArtemisINI remote = new(target);
                        result = ArtemisManagerAction.ArtemisINI.Merge(local, remote);

                    }
                    if (result != null)
                    {
                        result.Save(target);
                    }
                    retVal = true;
                }
            }
            return retVal;
        }
        public static string[] GetArtemisINIFileList()
        {
            ModManager.CreateFolder(ArtemisINIFolder);
            List<string> list = new();
            foreach (var fle in new DirectoryInfo(ArtemisINIFolder).GetFiles("*" + INIFileExtension))
            {
                list.Add(fle.Name);
            }
            return list.ToArray();
        }

        public static void SaveDMXCommandsFile(string sourceFile)
        {
            ModManager.CreateFolder(DMXCommandsFolder);
            FileInfo source = new(sourceFile);
            if (source.Exists)
            {
                source.CopyTo(Path.Combine(DMXCommandsFolder, source.Name), true);
            }
        }
        public static string[] GetDMXCommandsFileList()
        {
            ModManager.CreateFolder(DMXCommandsFolder);
            List<string> list = new();
            foreach (var fle in new DirectoryInfo(DMXCommandsFolder).GetFiles("*" + XMLFileExtension))
            {
                list.Add(fle.Name);
            }
            return list.ToArray();
        }
        public static void SaveControlsINIFile(string sourceFile)
        {
            ModManager.CreateFolder(ControlsINIFolder);
            FileInfo source = new(sourceFile);
            if (source.Exists)
            {
                source.CopyTo(Path.Combine(ControlsINIFolder, source.Name), true);
            }
        }
        public static string[] GetControlsINIFileList()
        {
            ModManager.CreateFolder(ControlsINIFolder);
            List<string> list = new();
            foreach (var fle in new DirectoryInfo(ControlsINIFolder).GetFiles("*" + INIFileExtension))
            {
                list.Add(fle.Name);
            }
            return list.ToArray();
        }
        public static string[] GetEngineeringPresetFiles()
        {
            List<string> retVal = new();
            ModManager.CreateFolder(EngineeringPresetsFolder);
            foreach (var fle in new DirectoryInfo(EngineeringPresetsFolder).GetFiles("*" + DATFileExtension))
            {
                retVal.Add(fle.Name);
            }
            if (retVal.Count == 0 && File.Exists(Path.Combine(ModItem.ActivatedFolder, ArtemisEngineeringFile)))
            {
                File.Copy(Path.Combine(ModItem.ActivatedFolder, ArtemisEngineeringFile), Path.Combine(EngineeringPresetsFolder, OriginalArtemisEngineeringFile));
                retVal.Add(OriginalArtemisEngineeringFile);
            }
            return retVal.ToArray();
        }
        public async static Task<Tuple<string, string, string>[]> GetExternaToolsLinks()
        {
            //\\ENDEAVER\artemis.russjudge.com\Tools\tools.txt line =< name >| file |< ownerwebsite >
            string[] info;
            List<Tuple<string, string, string>> links = new();
            //download from https://artemis.russjudge.com/Tools/tools.txt
            //  format will be:
            //  name|file|ownerwebsite
            //Artemis Mission Editor|missionEditor.msi|https://
            //be sure to keep the correct casing of the filename.
            try
            {
                using (HttpClient client = new())
                {
                    var data = await client.GetStringAsync(ExternalToolsURLFolder + "tools" + TXTFileExtension);
                    //line 1 = version
                    //line 2 = setup file to download.
                    info = data.Replace("\r", string.Empty).Split('\n');

                }
                foreach (var line in info)
                {
                    string[] item = line.Split('|');
                    if (item.Length > 2)
                    {
                        links.Add(new(item[0], item[1], item[2]));
                    }
                }

            }
            catch (Exception ex)
            {
            }
            return links.ToArray();
        }
        public async static Task<KeyValuePair<string, string>[]> GetArtemisUpgradeLinks()
        {
            //download from https://artemis.russjudge.com/software/artemisupgrades/artemisupgrades.txt
            //  format will be:
            //  version: url
            //2.1.1:artemis_V2_1_1.exe
            //be sure to keep the correct casing of the filename.
            string[] info;
            List<KeyValuePair<string, string>> links = new();
            try
            {
                using (HttpClient client = new())
                {
                    var data = await client.GetStringAsync(ArtemisUpgradesURLFolder + "artemisupgrades" + TXTFileExtension);
                    //line 1 = version
                    //line 2 = setup file to download.
                    info = data.Replace("\r", string.Empty).Split('\n');

                }
                foreach (var line in info)
                {
                    string[] item = line.Split(':');
                    if (item.Length > 1)
                    {
                        links.Add(new(item[0], item[1]));
                    }
                }

            }
            catch (Exception ex)
            {
            }

            return links.ToArray();
        }
        [Obsolete("Need to somehow get failure or success back to user.")]
        public static async void DownloadFile(string url, string target)
        {
            using HttpClient client = new();
            try
            {

                using var stream = await client.GetStreamAsync(url);
                if (stream != null)
                {
                    byte[] buffer = new byte[32768];
                    int bytesRead = 0;
                    if (stream != null)
                    {

                        using FileStream fs = new(target, FileMode.Create);
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, bytesRead);
                        }
                    }
                    else
                    {
                        /*
                        Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show("Error downloading " + file,
                            "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                        */
                    }
                }
                else
                {
                    /*
                    Dispatcher.Invoke(() =>
                    {

                        System.Windows.MessageBox.Show("Error downloading " + file,
                            "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);

                    });
                    */
                }
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show("Error downloading " + file + ":\r\n\r\n" + ex.Message,
                //                   "Artmis SBS Upgrade file download", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static bool CheckIfArtemisSnapshotNeeded(string installFolder)
        {
            bool matchFound = false;
            string? installedVersion = GetArtemisVersion(installFolder);
            if (!string.IsNullOrEmpty(installedVersion))
            {
                foreach (var mod in GetInstalledMods())
                {
                    if (mod.IsArtemisBase)
                    {
                        if (mod.Version == installedVersion)
                        {
                            matchFound = true;
                            break;
                        }
                    }
                }
            }
            else
            {
                matchFound = true;
            }
            return !matchFound;
        }
        public static bool IsModActive(ModItem mod)
        {
            return (File.Exists(Path.Combine(ModManager.DataFolder, mod.SaveFile)));
        }
        public static ModItem? ClearActiveFolder()
        {
            ModItem? baseArtemis = null;
            foreach (var mod in GetActivatedMods())
            {
                if (mod.IsArtemisBase)
                {
                    baseArtemis = mod;
                    break;
                }
            }
            DeleteAll(ModItem.ActivatedFolder);
            foreach (var mod in GetInstalledMods())
            {
                if (mod.IsActive)
                {
                    mod.IsActive = false;
                    mod.StackOrder = 0;
                    mod.Save();
                }
            }
            foreach (var mission in GetInstalledMissions())
            {
                if (mission.IsActive)
                {
                    mission.IsActive = false;
                    mission.StackOrder = 0;
                    mission.Save();
                }
            }

            foreach (var fle in new DirectoryInfo(ModManager.DataFolder).GetFiles("*" + SaveFileExtension))
            {
                fle.Delete();
            }
            if (baseArtemis != null)
            {
                baseArtemis.IsActive = false;
            }
            return baseArtemis;
        }
        public static void DeleteAll(string target)
        {
            if (Directory.Exists(target))
            {
                foreach (var dir in new DirectoryInfo(target).GetDirectories())
                {
                    DeleteAll(dir.FullName);
                    //if (dir.Exists)
                    //{
                    //    dir.Delete();
                    //}
                }
                foreach (var fle in new DirectoryInfo(target).GetFiles())
                {
                    fle.Delete();
                }
                if (Directory.Exists(target))
                {
                    Directory.Delete(target);
                }
            }
        }

        public static ModItem[] GetInstalledMissions()
        {
            var retVal = GetModList(ModItem.MissionInstallFolder);
            foreach (var item in retVal)
            {
                if (string.IsNullOrEmpty(item.SaveFile))
                {
                    item.Save();
                }

            }

            return retVal;
        }
        public static ModItem[] GetInstalledMods()
        {
            var retVal = GetModList(ModItem.ModInstallFolder);
            foreach (var item in retVal)
            {
                if (string.IsNullOrEmpty(item.SaveFile))
                {
                    item.Save();
                }
                item.IsActive = (File.Exists(Path.Combine(ModManager.DataFolder, item.SaveFile)));
            }

            return retVal;
        }
        public static ModItem[] GetActivatedMods()
        {
            return GetModList(ModManager.DataFolder);
        }
        private static ModItem[] GetModList(string path)
        {
            ModManager.CreateFolder(path);
            List<ModItem> installedMods = new();
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles("*" + SaveFileExtension))
            {
                ModItem? modItem = ModItem.LoadModItem(file.FullName);
                if (modItem != null)
                {
                    installedMods.Add(modItem);
                }
            }
            return installedMods.ToArray();
        }
        public static ModItem SnapshotInstalledArtemisVersion(string installFolder)
        {
            ModItem retVal = new();
            string? version = GetArtemisVersion(installFolder);
            retVal.RequiredArtemisVersion = version;
            retVal.Name = Artemis + " SBS";
            retVal.Author = "Thom Robertson";
            retVal.Version = version;
            retVal.Description = "Base " + Artemis;
            retVal.IsArtemisBase = true;
            //TODO: build hardcoded Guid list based on Artemis version.  Get comprehensive list of versions from the changes.txt file.
            if (version != null)
            {
                if (ArtemisVersionIdentifiers.TryGetValue(version, out Guid ModId))
                {
                    retVal.ModIdentifier = ModId;
                }
            }
            string target = Path.Combine(ModItem.ModInstallFolder, retVal.GetInstallFolder());

            ModManager.CopyFolder(installFolder, target);

            retVal.Save();
            if (version != null)
            {
                ModManager.CreateFolder(ArtemisINIFolder);
                System.IO.File.Copy(Path.Combine(installFolder, ArtemisManager.ArtemisINI), GetOriginalArtemisINIFilename(version), true);
            }
            return retVal;
        }
        public static string GetOriginalArtemisININame(string version)
        {
            return originalIdentifier + "_V" + version + INIFileExtension;
        }
        public static string GetOriginalArtemisINIFilename(string version)
        {
            return Path.Combine(ModManager.DataFolder, GetOriginalArtemisININame(version));
        }
        public static string GetOriginalArtemisINIFile(string installFolder)
        {
            string? version = GetArtemisVersion(installFolder);
            if (version != null)
            {
                return GetOriginalArtemisINIFilename(version);
            }
            else
            {
                return string.Empty;
            }
        }
        public static void VerifyArtemisINIBackup()
        {
            foreach (var mod in GetInstalledMods())
            {
                if (mod.IsArtemisBase)
                {
                    string installFolder = Path.Combine(ModItem.ModInstallFolder, mod.InstallFolder);
                    string target = GetOriginalArtemisINIFile(installFolder);
                    string target2 = Path.Combine(ArtemisINIFolder, new FileInfo(target).Name);
                    if (!string.IsNullOrEmpty(target))
                    {
                        string source = Path.Combine(installFolder, ArtemisINI);
                        if (File.Exists(source))
                        {
                            if (!File.Exists(target))
                            {
                                File.Copy(source, target, true);
                            }
                            if (!File.Exists(target2))
                            {
                                File.Copy(source, target2, true);
                            }
                        }
                    }
                }
            }
        }

        public static string? AutoDetectArtemisInstallPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsAutoDetectArtemisInstallPath();
            }
            else
            {
                return DetectArtemisInstallPath();
            }
        }

        private static string? DetectArtemisInstallPath()
        {
            string? retVal = null;
            foreach (var driv in DriveInfo.GetDrives())
            {
                FileInfo f = new(Path.Combine(driv.RootDirectory.FullName, "Program Files", Artemis, ArtemisEXE));
                if (f.Exists)
                {
                    retVal = f.DirectoryName;
                    break;
                }
                else
                {
                    f = new FileInfo(Path.Combine(driv.RootDirectory.FullName, "Program Files (x86)", Artemis, ArtemisEXE));
                    if (f.Exists)
                    {
                        retVal = f.DirectoryName;
                        break;
                    }
                }
            }
            retVal ??= SteamInfo.GetArtemisGameFolder();
            return retVal;
        }
        /// <summary>
        /// Gets the path to where Artemis is installed, or null if it can't be found.
        /// </summary>
        /// <returns>Path to Artemis, or null if not found</returns>
        [SupportedOSPlatform("windows")]
        private static string? WindowsAutoDetectArtemisInstallPath()
        {
            string? retVal;
            try
            {
                string[] RegistryPath = RegistryArtemisInstallLocation.Split('\\');

                RegistryKey? wrkKey = Registry.LocalMachine;
                if (wrkKey != null)
                {
                    for (int i = 1; i < RegistryPath.Length; i++)
                    {
                        wrkKey = wrkKey.OpenSubKey(RegistryPath[i]);
                        if (wrkKey == null)
                        {
                            return null;
                        }

                    }
                    retVal = wrkKey.GetValue(string.Empty) as string;
                    if (retVal != null)
                    {
                        FileInfo f = new(retVal);
                        if (f.Exists)
                        {
                            retVal = f.DirectoryName;
                        }
                        else
                        {
                            retVal = DetectArtemisInstallPath();
                        }
                    }
                }
                else
                {
                    retVal = DetectArtemisInstallPath();
                }
            }
            catch
            {
                retVal = null;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the Artemis version from the changes.txt file.
        /// </summary>
        /// <param name="installPath"></param>
        /// <returns>The version of Artemis in the specified path or null if not found/determined</returns>
        public static string? GetArtemisVersion(string installPath)
        {
            string? retVal = null;
            if (installPath != null)
            {
                string changesFile = Path.Combine(installPath, ChangesTXT);
                if (File.Exists(changesFile))
                {

                    using StreamReader sr = new(changesFile);
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("changes for", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int i = line.IndexOf(" V");
                            if (i > -1)
                            {
                                int j = line.IndexOf(" ", i + 1);
                                if (j < 0)
                                {
                                    j = line.Length;
                                }
                                retVal = line.Substring(i + 2, j - (i + 2));
                            }
                            break;
                        }
                    }
                    if (retVal != null)
                    {
                        int k = retVal.IndexOf(".");
                        if (retVal.IndexOf(".", k + 1) == -1)
                        {
                            retVal += ".0";
                        }
                        if (retVal == "2.8.0")
                        {
                            FileInfo f = new(Path.Combine(installPath, ArtemisEXE));
                            if (f.LastWriteTime.Year >= 2022 && f.LastWriteTime.Year <= 2023)
                            {
                                retVal = "2.8.1";
                            }
                        }
                    }
                }
            }
            return retVal;
        }
        public static bool IsArtemisRunning()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(Artemis);
            return (processes.Length > 0);
        }
        public static bool IsRunningArtemisUnderMyControl()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(Artemis);
            foreach (var pro in processes)
            {
                if (pro.MainModule != null)
                {
                    if (pro.MainModule.FileName.Equals(Path.Combine(ModItem.ActivatedFolder, ArtemisEXE)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void StartArtemis()
        {
            if (!IsArtemisRunning())
            {
                string target = Path.Combine(ModItem.ActivatedFolder, ArtemisEXE);
                if (File.Exists(target))
                {
                    ProcessStartInfo startInfo = new(target)
                    {
                        WorkingDirectory = Path.GetDirectoryName(target)
                    };

                    runningArtemisProcess = System.Diagnostics.Process.Start(startInfo);
                    if (runningArtemisProcess != null)
                    {
                        runningArtemisProcess.EnableRaisingEvents = true;
                        runningArtemisProcess.Disposed += RunningArtemisProcess_Disposed;
                        runningArtemisProcess.Exited += RunningArtemisProcess_Exited;
                    }
                }
            }
        }
        private static void RunningArtemisProcess_Exited(object? sender, EventArgs e)
        {
            runningArtemisProcess?.Dispose();
        }

        private static void RunningArtemisProcess_Disposed(object? sender, EventArgs e)
        {
            runningArtemisProcess = null;
        }
        public static void StopArtemis()
        {
            try
            {
                runningArtemisProcess?.Kill(true);
            }
            catch { }
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(Artemis);
                foreach (var pro in processes)
                {
                    pro.Kill(true);
                }
            }
            catch { }
        }
    }
}
