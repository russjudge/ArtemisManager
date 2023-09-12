using AMCommunicator;
using AMCommunicator.Messages;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

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
        public static event EventHandler<CommunicationMessageEventArgs>? ActivatePopupMessage;
        public static void RaiseStatusUpdated(string? host, string message)
        {
            ActivatePopupMessage?.Invoke(null, new CommunicationMessageEventArgs(host, message));
        }
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
                sw.Write(file);
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
                    else
                    {
                        result = local;
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
                list.Add(fle.FullName);
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
                list.Add(fle.FullName);
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
        public static bool RenameSettingsFile(string folder, string extension, string oldName, string newName)
        {
            if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(oldName) && newName != oldName)
            {
                string source = ResolveFilename(folder, oldName, extension);
                string target = ResolveFilename(folder, newName, extension);
                if (File.Exists(target))
                {
                    return false;
                }
                else
                {
                    File.Move(source, target, true);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool RestoreDefaultOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile fileType, string data)
        {
            string targetFile = string.Empty;
            switch (fileType)
            {
                case SendableStringPackageFile.controlsINI:
                    targetFile = ResolveFilename(ModItem.ActivatedFolder, controlsINI, INIFileExtension);
                   
                    break;
                case SendableStringPackageFile.DMXCommandsXML:
                    targetFile = ResolveFilename(Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder), DMXCommands, XMLFileExtension);
                    break;
                default:
                    break;
            }
            if (System.IO.File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }
            using (StreamWriter sw = new(targetFile))
            {
                sw.Write(data);
            }
            return true;
        }
        public static bool RenameOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile fileType, string oldName, string newName)
        {
            bool retVal = false;
            switch (fileType)
            {
                case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                    retVal = RenameSettingsFile(ControlsINIFolder, INIFileExtension, oldName, newName);
                    if (retVal)
                    {
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                    }
                    break;
                case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                    retVal = RenameSettingsFile(DMXCommandsFolder, XMLFileExtension, oldName, newName);
                    if (retVal)
                    {
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                    }
                    break;
                default:
                    retVal = RenameSettingsFile(DMXCommandsFolder, XMLFileExtension, oldName, newName);
                    if (retVal)
                    {
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                    }
                    break;
            }
            return retVal;
        }
        public static bool RenameArtemisINIFile(string oldName, string newName)
        {
            var retVal = RenameSettingsFile(ArtemisINIFolder, INIFileExtension, oldName, newName);
            if (retVal)
            {
                Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
            }
            return retVal;
        }
        public static bool RenameEngineeringPresetsFile(string oldName, string newName)
        {
            var retVal = RenameSettingsFile(EngineeringPresetsFolder, DATFileExtension, oldName, newName);
            if (retVal)
            {
                Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfEngineeringPresets, string.Empty, ArtemisManager.GetEngineeringPresetFiles());
            }
            return retVal;
        }
        public static bool DeleteArtemisINIFile(string name)
        {
            string target = ResolveFilename(ArtemisINIFolder, name, INIFileExtension);
            if (File.Exists(target))
            {
                File.Delete(target);
                Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
                return true;
            }
            else
            {
                return false;
            }
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
        public static string GetArtemisINIData(string name)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, name + ArtemisManager.INIFileExtension);
                if (File.Exists(source))
                {
                    using (var data =  new StreamReader(source))
                    {
                        retVal = data.ReadToEnd();
                    }
                }
            }
            return retVal;
        }
        public static string GetEngineeringPresetData(string name)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, name + ArtemisManager.DATFileExtension);
                if (File.Exists(source))
                {
                    PresetsFile file = new(source);
                    retVal = file.GetSerializedString();
                }
            }
            return retVal;
        }
        public static string GetControlsINIData(string name)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                string source = System.IO.Path.Combine(ArtemisManager.ControlsINIFolder, name + ArtemisManager.INIFileExtension);
                if (File.Exists(source))
                {
                    using (var data = new StreamReader(source))
                    {
                        retVal = data.ReadToEnd();
                    }
                }
            }
            return retVal;
        }
        public static string GetDMXCommandsData(string name)
        {
            string retVal = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                string source = System.IO.Path.Combine(ArtemisManager.DMXCommandsFolder, name + ArtemisManager.XMLFileExtension);
                if (File.Exists(source))
                {
                    using (var data = new StreamReader(source))
                    {
                        retVal = data.ReadToEnd();
                    }
                }
            }
            return retVal;
        }
        public static ArtemisINI? GetArtemisINI(string name)
        {
            ArtemisINI? retVal = null;
            if (!string.IsNullOrEmpty(name))
            {
                string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, name + ArtemisManager.INIFileExtension);
                if (File.Exists(source))
                {
                    retVal = new ArtemisINI(source);
                }
            }
            return retVal;
        }
        public static ModItem SnapshotInstalledArtemisVersion(string installFolder, string artemisEXE = ArtemisManager.ArtemisEXE, bool isArtemisCosmos = false)
        {
            ModItem retVal = new();
            string? version = GetArtemisVersion(installFolder);
            retVal.RequiredArtemisVersion = version;
            retVal.Name = Artemis + " SBS";
            retVal.Author = "Thom Robertson";
            retVal.Version = version;
            retVal.Description = "Base " + Artemis;

            retVal.IsArtemisBase = true;
            retVal.ArtemisEXE = ArtemisManager.ArtemisEXE;
            retVal.IsArtemisCosmos = isArtemisCosmos;
            if (retVal.IsArtemisCosmos)
            {
                retVal.Description += " Cosmos";
            }
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
        public static bool RestoreEngineeringPresetsToDefault()
        {
            System.IO.FileInfo fle = new(System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile));
            if (fle.Exists)
            {
                fle.Delete();
            }
            return true;
        }
        public static bool DeleteOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile fileType, string name)
        {
            string sourceFolder = string.Empty;
            string souceExtension = string.Empty;
            switch (fileType)
            {
                case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                    sourceFolder = ControlsINIFolder;
                    souceExtension = INIFileExtension;
                    break;
                case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                    sourceFolder = DMXCommandsFolder;
                    souceExtension = XMLFileExtension;
                    break;
            }
            string source = ResolveFilename(sourceFolder, name, souceExtension);
            if (File.Exists(source))
            {
                File.Delete(source);
                switch (fileType)
                {
                    case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfControLINIFiles, string.Empty, ArtemisManager.GetControlsINIFileList());
                        break;
                    case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                        Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfDMXCommandfiles, string.Empty, ArtemisManager.GetDMXCommandsFileList());
                        break;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool ActivateOtherSettingsFile(AMCommunicator.Messages.SendableStringPackageFile fileType, string name)
        {
            string target = string.Empty;
            string source = string.Empty;
            switch (fileType)
            {
                case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                    source = ResolveFilename(DMXCommandsFolder, name, XMLFileExtension);
                    target = System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder, ArtemisManager.DMXCommands);
                    ModManager.CreateFolder(System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder));
                    break;
                case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                    source = ResolveFilename(ControlsINIFolder, name, INIFileExtension);
                    target = System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.controlsINI);
                    break;
                default:
                    return false;
            }
            System.IO.File.Copy(source, target, true);
            return true;
        }
        public static bool ActivateArtemisINIFile(string name)
        {
            
            string sourceName = name;
            if (!sourceName.Contains('.'))
            {
                sourceName += ArtemisManager.INIFileExtension;
            }
            string source = System.IO.Path.Combine(ArtemisManager.ArtemisINIFolder, sourceName);
            if (System.IO.File.Exists(source))
            {
                File.Copy(source, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisINI), true);
                RaiseStatusUpdated(null, "Activated artemis.ini file");
                //Network.Current?.SendInformation(IPAddress.Any, RequestInformationType.ListOfArtemisINIFiles, string.Empty, ArtemisManager.GetArtemisINIFileList());
                return true;
            }
            else
            {
                RaiseStatusUpdated(null, string.Format("Unable to activate file.  {0} not found", source));
                return false;
            }
        }
        public static bool ActivateEngineeringPresetFile(string name)
        {
            string source = ResolveFilename(EngineeringPresetsFolder, name, DATFileExtension);
            if (System.IO.File.Exists(source))
            {
                File.Copy(source, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
                RaiseStatusUpdated(null, "Activated Engineering Presets file");
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string ResolveFilename(string sourceFolder, string name, string extension)
        {
            string sourceName = name;
            if (!sourceName.EndsWith(extension))
            {
                sourceName += extension;
            }
            if (sourceName.Contains("\\") || sourceName.Contains("/"))
            {
                sourceName = new FileInfo(sourceName).Name;
            }

            string source = System.IO.Path.Combine(sourceFolder, sourceName);
            return source;
        }
       
        public static bool DeleteEngineeringPresetsFile(string name)
        {
            string source = ResolveFilename(EngineeringPresetsFolder, name, DATFileExtension);
            if (File.Exists(source))
            {
                File.Delete(source);
                return true;
            }
            else
            {
                return false;
            }
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
        public static bool RestoreArtemisINIToDefault(string data)
        {
            using (StreamWriter sw = new(Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisINI)))
            {
                sw.Write(data);
            }
            return true;
        }
        public static void VerifyArtemisINIBackup()
        {
            ModManager.CreateFolder(ArtemisINIFolder);
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
            if (string.IsNullOrEmpty(retVal))
            {
                retVal = SteamInfo.GetArtemisCosmosGameFolder();
            }
            return retVal;
        }
        [SupportedOSPlatform("windows")]
        public static void SetServerIP(IPAddress? address)
        {
            var wrkKey1 = Registry.CurrentUser.OpenSubKey("Software");
            if (wrkKey1 != null)
            {
                try
                {
                    var wrkKey = wrkKey1.OpenSubKey("ArtemisSBS", true);
                    if (wrkKey == null)
                    {
                        wrkKey = wrkKey1.CreateSubKey("ArtemisSBS", true);
                    }

                    if (address == null)
                    {
                        wrkKey.SetValue("IPAddress", string.Empty);
                    }
                    else
                    {
                        wrkKey.SetValue("IPAddress", address.ToString());
                    }
                }
                catch (Exception ex)
                {

                }
            }
            
        }
        public static bool GetIsMainScreenServer()
        {
            if (OperatingSystem.IsWindows())
            {
                var address = GetServerIP();
                if (address == null)
                {
                    return false;
                }
                else
                {
                    if (address.ToString() == IPAddress.Loopback.ToString())
                    {
                        return true;
                    }
                    else
                    {
                        return (address.ToString() == Network.GetMyIP()?.ToString());
                    }
                }
            }
            else
            {
                return false;
            }
        }
        [SupportedOSPlatform("windows")]
        public static IPAddress? GetServerIP()
        {
            IPAddress? retVal = null;
            var wrkKey = Registry.CurrentUser.OpenSubKey("Software");
            if (wrkKey != null)
            {
                wrkKey = wrkKey.OpenSubKey("ArtemisSBS", true);
                if (wrkKey != null)
                {
                    var result = wrkKey.GetValue("IPAddress", null);
                    if (result != null)
                    {
                        IPAddress.TryParse(result.ToString(), out retVal);
                    }
                }
            }
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
            //TODO: Modify to handle Artemis Cosmos.
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
                    if (pro.MainModule.FileName.Equals(GetCosmosEXE(ModItem.ActivatedFolder)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static string GetCosmosEXE(string folder)
        {
            string retVal = string.Empty;
            foreach (var file in new DirectoryInfo(folder).GetFiles())
            {
                if (file.Name.ToLowerInvariant().StartsWith("artemis") && file.Extension.ToLowerInvariant() == ".exe")
                {
                    retVal = file.Name;
                    break;
                }
            }
            return retVal;
        }
        public static void StartArtemis()
        {
            if (!IsArtemisRunning())
            {

                string target = Path.Combine(ModItem.ActivatedFolder, ArtemisEXE);
                if (!File.Exists(target))
                {
                    target = GetCosmosEXE(ModItem.ActivatedFolder);
                }
                if (!string.IsNullOrEmpty(target) && File.Exists(target))
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
