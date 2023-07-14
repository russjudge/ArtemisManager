using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisManager
    {
        public static readonly Dictionary<string, Guid> ArtemisVersionIdentifiers = new();
        public const string RegistryArtemisInstallLocation = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\windows\\CurrentVersion\\App Paths\\Artemis.exe";
        
        public readonly static string ActivatedModsTrackingFile = Path.Combine(ModManager.DataFolder, "ActivatedModsTracking.dat");
        public const string ArtemisEXE = "Artemis.exe";
        public const string SaveFileExtension = ".json";

        static System.Diagnostics.Process? runningArtemisProcess = null;

        static ArtemisManager()
        {
            //Fixing Artemis versions to specific Guids.  Same thing will be done to mods eventually.
            ArtemisVersionIdentifiers.Add("2.8", new Guid("D141B467-1A2F-48CE-8BDF-3540BDC48215"));
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
        public static ModItem? ClearActiveFolder()
        {
            var baseArtemis = DeleteAll(ModItem.ActivatedFolder);
            foreach (var mod in GetInstalledMods())
            {
                if (mod.IsActive)
                {
                    mod.IsActive = false;
                    mod.Save();
                }
            }
            return baseArtemis;
        }
        public static ModItem? DeleteAll(string target)
        {
            ModItem? mod = null;
            if (Directory.Exists(target))
            {
                foreach (var dir in new DirectoryInfo(target).GetDirectories())
                {
                    DeleteAll(dir.FullName);
                    dir.Delete();
                }
                foreach (var fle in new DirectoryInfo(target).GetFiles())
                {
                    if (target == ModItem.ActivatedFolder)
                    {
                        if (fle.Extension == ".json")
                        {
                            var activatedMod = ModItem.LoadModItem(fle.FullName);

                            if (activatedMod != null && activatedMod.IsArtemisBase)
                            {
                                mod = activatedMod;
                            }
                        }
                    }
                    fle.Delete();
                }
            }
            return mod;
        }
        
        public static ModItem[] GetInstalledMods()
        {
            var retVal = GetModList(ModItem.ModInstallFolder);
            foreach (var item in retVal)
            {
                item.IsActive = (File.Exists(Path.Combine(ModItem.ActivatedFolder, item.LocalIdentifier.ToString() + SaveFileExtension)));
            }
            
            return retVal;
        }
        public static ModItem[] GetActivatedMods()
        {
            return GetModList(ModItem.ActivatedFolder);
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
            string target;
            if (retVal.ModIdentifier == Guid.Empty)
            {
                target = "ArtemisV" + version;
            }
            else
            {
                target = retVal.ModIdentifier.ToString();
            }
            target = Path.Combine(ModItem.ModInstallFolder, target);
            retVal.InstallFolder = target;
            ModManager.CopyFolder(installFolder, target);
            retVal.Save(target + SaveFileExtension);
            
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
                }
            }
            return retVal;
        }
        public static bool IsArtemisRunning()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(ArtemisEXE);
            return (processes.Length > 0);
        }
        public static bool IsRunningArtemisUnderMyControl()
        {
            var processes = System.Diagnostics.Process.GetProcessesByName(ArtemisEXE);
            foreach (var pro in processes)
            {
                if (pro.StartInfo.FileName.StartsWith(ModItem.ActivatedFolder))
                {
                    return true;
                    
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
                    runningArtemisProcess = System.Diagnostics.Process.Start(target);
                    runningArtemisProcess.Disposed += RunningArtemisProcess_Disposed;
                    runningArtemisProcess.Exited += RunningArtemisProcess_Exited;

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
            var processes = System.Diagnostics.Process.GetProcessesByName(ArtemisEXE);
            foreach (var pro in processes)
            {
                pro.Kill(true);
            }
        }
    }
}
