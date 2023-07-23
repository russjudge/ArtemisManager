using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisManager
    {
        public static readonly Dictionary<string, Guid> ArtemisVersionIdentifiers = new();
        public const string ArtemisEXE = "Artemis.exe";
        public const string RegistryArtemisInstallLocation = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\windows\\CurrentVersion\\App Paths\\" + ArtemisEXE;
        
        public readonly static string ActivatedModsTrackingFile = Path.Combine(ModManager.DataFolder, "ActivatedModsTracking.dat");
        
        public const string SaveFileExtension = ".json";

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
        
        public static bool CheckIfArtemisSnapshotNeeded(string installFolder)
        {
            bool matchFound = false;
            string? installedVersion = GetArtemisVersion(installFolder);
            if (!string.IsNullOrEmpty(installedVersion) )
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
            retVal.Name = "Artemis SBS";
            retVal.Author = "Thom Robertson";
            retVal.Version = version;
            retVal.Description = "Base Artemis";
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
            
            return retVal;
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
                FileInfo f = new(Path.Combine(driv.RootDirectory.FullName, "Program Files", "Artemis", ArtemisEXE));
                if (f.Exists)
                {
                    retVal = f.DirectoryName;
                    break;
                }
                else
                {
                    f = new FileInfo(Path.Combine(driv.RootDirectory.FullName, "Program Files (x86)", "Artemis", ArtemisEXE));
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
                        FileInfo f = new (retVal);
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
                string changesFile = Path.Combine(installPath, "changes.txt");
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
                                int j = line.IndexOf(" ", i+1);
                                if (j < 0)
                                {
                                    j = line.Length;
                                }
                                retVal = line.Substring(i + 2, j - (i + 2));
                            }
                            break;
                        }
                    }
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
            return retVal;
        }
        public static bool IsArtemisRunning()
        {
            var processes =  System.Diagnostics.Process.GetProcessesByName("Artemis");
            //bool found = false;
            //foreach (var process in processes)
            //{
            //    if (process.MainModule.FileName.Equals(Path.Combine(ModItem.ActivatedFolder, "Artemis.exe"))
            //    {
            //        found = true;
            //        break;
            //    }
                
            //}
            return (processes.Length > 0);
        }
        public static bool IsRunningArtemisUnderMyControl()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName("Artemis");
            foreach (var pro in processes)
            {
                if (pro.MainModule != null)
                {
                    if (pro.MainModule.FileName.Equals(Path.Combine(ModItem.ActivatedFolder, "Artemis.exe")))
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
                    ProcessStartInfo startInfo = new ProcessStartInfo(target);
                    startInfo.WorkingDirectory = Path.GetDirectoryName(target);
                    
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
            runningArtemisProcess?.Kill(true);
            var processes = System.Diagnostics.Process.GetProcessesByName("Artemis");
            foreach (var pro in processes)
            {
                pro.Kill(true);
            }
        }
    }
}
