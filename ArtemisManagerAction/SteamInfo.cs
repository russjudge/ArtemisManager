using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace ArtemisManagerAction
{
    public class SteamInfo
    {
        const string SteamAppSubfolder1 = "steamapps";
        const string SteamAppCommonFolder = "common";
        const string SteamAppManifestFile = "appmanifest_" + ArtemisAppKey + ".acf";
        const string SteamAppManifestCosmosFile = "appmanifest_" + ArtemisCosmosAppKey + ".acf";
        const string SteamAppInstallFolderName = "\"installdir\"";  //Example: "installdir"		"Archon" (2 tabs).  Preprocess file by replace tabs with spaces, reducing to 1 space. read lines until installdir found.
        const string ArtemisAppKey = "247350";
        const string ArtemisCosmosAppKey = "2467840";
        const string SteamRegistryKey = @"SOFTWARE\Valve\Steam";
        const string SteamRegistrySteamPathValue = "SteamPath";
        const string SteamAppsRelativeFolder = "steamApps";
        const string SteamLibraryFolderList = "libraryFolders.vdf";
        private static string? GetSteamInstallFolder()
        {
            if (OperatingSystem.IsWindows())
            {
                return GetWindowsSteamInstallFolder();
            }
            else if (OperatingSystem.IsLinux())
            {
                return GetLinuxSteamInstallFolder();
            }
            return null;
        }
        [SupportedOSPlatform("linux")]
        private static string? GetLinuxSteamInstallFolder()
        {
            return null;
        }
        [SupportedOSPlatform("windows")]
        private static string? GetWindowsSteamInstallFolder()
        {
            //LastErrorMessage = "";
            //TODO: Create a Linux version of this method.
            string? retVal;

            try
            {
                var reg = Registry.CurrentUser.OpenSubKey(SteamRegistryKey);
                if (reg == null)
                {
                    retVal = null;
                }
                else
                {
                    var steamPath = reg.GetValue(SteamRegistrySteamPathValue);
                    retVal = steamPath as string;
                }
            }
            catch (Exception ex)
            {
                retVal = null;
                //LastErrorMessage = ex.Message;
            }

            return retVal;
        }
        public static string? GetArtemisGameFolder()
        {
            string? retVal = null;
            foreach (var folder in GetSteamLibraryFolders())
            {
                string manifestFolder = Path.Combine(folder, SteamAppSubfolder1, SteamAppManifestFile);
                if (File.Exists(manifestFolder))
                {
                    string data;
                    using (StreamReader sr = new StreamReader(manifestFolder))
                    {
                        data = sr.ReadToEnd();
                    }
                    var i = data.IndexOf(SteamAppInstallFolderName) + SteamAppInstallFolderName.Length;
                    i = data.IndexOf("\"", i) + 1;
                    var j = data.IndexOf("\"", i);
                    retVal = data.Substring(i, j - i);
                    if (File.Exists(Path.Combine(retVal, ArtemisManager.ArtemisEXE)))
                    {
                        break;
                    }
                    else
                    {
                        retVal = null;
                    }
                }
            }
            return retVal;
        }
        public static string? GetArtemisCosmosGameFolder()
        {
            string? retVal = null;
            foreach (var folder in GetSteamLibraryFolders())
            {
                string manifestFolder = Path.Combine(folder, SteamAppSubfolder1, SteamAppManifestCosmosFile);
                if (File.Exists(manifestFolder))
                {
                    string data;
                    using (StreamReader sr = new StreamReader(manifestFolder))
                    {
                        data = sr.ReadToEnd();
                    }
                    var i = data.IndexOf(SteamAppInstallFolderName) + SteamAppInstallFolderName.Length;
                    i = data.IndexOf("\"", i) + 1;
                    var j = data.IndexOf("\"", i);
                    retVal = data.Substring(i, j - i);
                    if (File.Exists(Path.Combine(retVal, ArtemisManager.ArtemisEXE)))
                    {
                        break;
                    }
                    else
                    {
                        retVal = null;
                    }
                }
            }
            return retVal;
        }
        
        private static string[] GetSteamLibraryFolders()
        {
            List<string> retVal = new();

            var steamFolder = GetSteamInstallFolder();
            if (steamFolder != null)
            {
                if (Directory.Exists(steamFolder))
                {
                    retVal.Add(steamFolder);
                    string libraryListFileName = Path.Combine(steamFolder, SteamAppsRelativeFolder, SteamLibraryFolderList);

                    if (File.Exists(libraryListFileName))
                    {
                        var info = new SteamInfo(libraryListFileName);

                        foreach (var key in info.GetKeys())
                        {
                            if (int.TryParse(key, out int libraryNumber))
                            {
                                var value = info.GetValue(key);
                                if (value != null)
                                {
                                    retVal.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            return retVal.ToArray();
        }
        public SteamInfo(string file)
        {
            Filename = file;
            using (var strm = new StreamReader(file))
            {
                rawData = strm.ReadToEnd();
            }
            FixRawData();

            ProcessRawData();
        }

        void FixRawData()
        {
            if (rawData != null)
            {
                rawData = rawData.Replace("\r", string.Empty).Replace("\t", " ");
                StringBuilder sb = new();
                foreach (var line in rawData.Split('\n'))
                {
                    sb.Append(line);
                    if (line.EndsWith("\"") || line == "{" || line == "}")
                    {
                        sb.Append('\n');
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                rawData = sb.ToString();
            }
        }

        /*
         
"LibraryFolders"
{
	"TimeNextStatsReport"		"1611876812"
	"ContentStatsID"		"5688185081579020048"
	"1"		"D:\\StLibrary"
	"2"		"B:\\SteamLibrary"
	"3"		"G:\\Steam"
}

LibraryFolders
        Keys:
            1
            2
            3

publish_data
        Keys: 
            title
            publish_time_readable

        */
        void ProcessRawData()
        {
            //First line is the object name
            if (rawData != null)
            {
                string[] lines = rawData.Split('\n');
                if (lines.Length > 0)
                {
                    ObjectName = lines[0].Substring(1, lines[0].Length - 2);
                    if (lines.Length > 2)
                    {
                        for (int i = 2; i < lines.Length - 1; i++)
                        {
                            string line = lines[i].Replace("\t", " ").Trim();
                            int start = 1;
                            int end = 1;
                            if (line != "}")
                            {
                                do
                                {

                                } while (line.Substring(++end, 1) != "\"");
                                string key = line.Substring(start, end - start);

                                start = end + 1;
                                do
                                {

                                } while (line.Substring(++start, 1) != "\"");
                                start++;
                                end = start;
                                if (line.Substring(end, 1) != "\"")
                                {
                                    do
                                    {

                                    } while (line.Substring(++end, 1) != "\"");
                                }
                                string value = string.Empty;
                                if (end > start)
                                {
                                    value = line.Substring(start, end - start);
                                    value = value.Replace(@"\\", @"\");
                                }
                                MainData.Add(key, value);
                            }

                        }
                    }
                }
            }
        }
        readonly Dictionary<string, string> MainData = new();
        string? rawData = null;
        public string Filename
        {
            get;
            private set;
        }
        public string? GetValue(string key)
        {
            string? retVal = null;
            if (MainData.ContainsKey(key))
            {
                retVal = MainData[key];
            }
            return retVal;
        }
        public string[] GetKeys()
        {
            List<string> retVal = new();
            foreach (var key in MainData.Keys)
            {
                retVal.Add(key);
            }
            return retVal.ToArray();
        }
        public bool HasKey(string key)
        {
            return MainData.ContainsKey(key);
        }
        public string? ObjectName { get; private set; }
    }
}
