using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisManager
    {
        public const string RegistryArtemisInstallLocation = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\windows\\CurrentVersion\\App Paths\\Artemis.exe";
        public readonly static string ActivatedFolder = Path.Combine(ModManager.DataFolder, "Activated");
        public readonly static string ActivatedModsTrackingFile = Path.Combine(ModManager.DataFolder, "ActivatedModsTracking.dat");
        public const string ArtemisEXE = "Artemis.exe";
        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static ModItem[] GetInstalledMods()
        {
            var retVal = GetModList(ModManager.ModInstallFolder);
            foreach (var item in retVal)
            {
                item.IsActive = (File.Exists(Path.Combine(ActivatedFolder, item.LocalIdentifier.ToString() + ".json")));
            }
            
            return retVal;
        }
        public static ModItem[] GetActivatedMods()
        {
            return GetModList(ActivatedFolder);
        }
        private static ModItem[] GetModList(string path)
        {
            CreateFolder(path);
            List<ModItem> installedMods = new();
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles("*.json"))
            {
                using StreamReader sr = new(file.FullName);
                string data = sr.ReadToEnd();
                ModItem? modItem = ModItem.GetModItem(data);
                if (modItem != null)
                {
                    installedMods.Add(modItem);
                }
            }
            return installedMods.ToArray();
        }
        public static void SnapshotInstalledArtemisVersion()
        {

        }

        public static string? AutoDetectArtemisInstallPath()
        {
            if (OperatingSystem.IsWindows())
            {
                return WindowsAutoDetectArtemisInstallPath();
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Gets the path to where Artemis is installed, or null if it can't be found.
        /// </summary>
        /// <returns>Path to Artemis, or null if not found</returns>
        [SupportedOSPlatform("windows")]
        private static string? WindowsAutoDetectArtemisInstallPath()
        {
            string? retVal = null;
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
                        FileInfo f = new FileInfo(retVal);
                        if (f.Exists)
                        {
                            retVal = f.DirectoryName;
                        }
                        else
                        {
                            f = new FileInfo(@"C:\Program Files\Artemis\Artemis.exe");
                            if (f.Exists)
                            {
                                retVal = f.DirectoryName;
                            }
                            else
                            {
                                f = new FileInfo(@"C:\Program Files (x86)\Aretmis\Artemis.exe");
                                if (f.Exists)
                                {
                                    retVal = f.DirectoryName;
                                }
                                else
                                {
                                    retVal = null;
                                }
                            }
                        }
                    }
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

                    using (StreamReader sr = new StreamReader(changesFile))
                    {
                        string? line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line.Contains("changes for", StringComparison.InvariantCultureIgnoreCase))
                            {
                                int i = line.IndexOf(" V");
                                if (i > -1)
                                {
                                    int j = line.IndexOf(" ");
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
                if (pro.StartInfo.FileName.StartsWith(ActivatedFolder))
                {

                }
                else
                {
                    return false;
                }
            }
            return true;
        }

    }
}
