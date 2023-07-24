using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ModItem : INotifyPropertyChanged
    {
        public static readonly string ModInstallFolder = Path.Combine(ModManager.DataFolder, "InstalledMods");
        public static readonly string MissionInstallFolder = Path.Combine(ModManager.DataFolder, "InstalledMissions");
        
        public readonly static string ActivatedFolder = Path.Combine(ModManager.DataFolder, "Activated");
        public static readonly string MissionFolderPath = Path.Combine(ActivatedFolder, "dat", "Missions");
        public ModItem()
        {
            
        }
        /// <summary>
        /// Creates a copy ModItem that is activated.
        /// </summary>
        public ModItem Activate()
        {
            if (IsArtemisBase)
            {
                ArtemisManager.ClearActiveFolder();
            }
            ModItem item;
            string targetPath = ActivatedFolder;
            
            if (string.IsNullOrEmpty(SaveFile))
            {
                SaveFile = GetSaveFile();
            }
            
            if (!IsMission)
            {

                StackOrder = new DirectoryInfo(ModManager.DataFolder).GetFiles("*" + ArtemisManager.SaveFileExtension).Length;

                item = new()
                {
                    Key = Key,
                    ModIdentifier = ModIdentifier,

                    Name = Name,
                    Description = Description,
                    Version = Version,
                    RequiredArtemisVersion = RequiredArtemisVersion,
                    Author = Author,
                    IsArtemisBase = IsArtemisBase,
                    localIdentifier = LocalIdentifier,
                    ReleaseDate = ReleaseDate,
                    isActive = true,
                    InstallFolder = InstallFolder,
                    PackageFile = PackageFile,
                    SaveFile = SaveFile
                };
                
                item.Save(Path.Combine(ModManager.DataFolder, SaveFile));
            }
            else
            {
                item = this;
                targetPath = MissionFolderPath;
                
            }
            string sourcePath = Path.Combine(GetFullSavePath(), InstallFolder);
            ModManager.CopyFolder(sourcePath, targetPath);
            this.IsActive = true;
            this.Save();

            return item;
        }
        
        public bool Uninstall()
        {
            bool retVal = false;
            if (!IsActive || IsMission || IsArtemisBase)
            {
                ArtemisManager.DeleteAll(Path.Combine(GetFullSavePath(), this.InstallFolder));
                if (string.IsNullOrEmpty(SaveFile))
                {
                    SaveFile = GetSaveFile();
                }
                if (!string.IsNullOrEmpty(SaveFile) && File.Exists(Path.Combine(GetFullSavePath(), SaveFile)))
                {
                    File.Delete(Path.Combine(GetFullSavePath(), SaveFile));
                }
               

                retVal = true;
            }
            return retVal;
        }
        private string key = string.Empty;
        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                DoChanged();
            }
        }

        private Guid modIdentifier = Guid.Empty;
        public Guid ModIdentifier
        {
            get { return modIdentifier; }
            set
            {
                modIdentifier = value;
                DoChanged();
            }
        }

        private string name = string.Empty;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                DoChanged();
            }
        }
        private string description = string.Empty;
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                DoChanged();
            }
        }
        private string author = string.Empty;
        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                DoChanged();
            }
        }
        private string? version = string.Empty;
        public string? Version
        {
            get { return version; }
            set
            {
                version = value;
                DoChanged();
            }
        }
        private DateTime releaseDate = DateTime.Today;
        public DateTime ReleaseDate
        {
            get { return releaseDate; }
            set
            {
                releaseDate = value;
                DoChanged();
            }
        }
        private string? requiredAretmisVersion ="2.8.0";
        public string? RequiredArtemisVersion
        {
            get { return requiredAretmisVersion; }
            set
            {
                requiredAretmisVersion = value;
                DoChanged();
            }
        }
        private string[] compatibleArtemisVersions = Array.Empty<string>();
        public string[] CompatibleArtemisVersions
        {
            get { return compatibleArtemisVersions; }
            set
            {
                compatibleArtemisVersions = value;
                DoChanged();
            }
        }
        private bool isAretmisBase = false;
        public bool IsArtemisBase
        {
            get { return isAretmisBase; }
            set
            {
                isAretmisBase = value;
                DoChanged();
            }
        }
        private string installFolder = string.Empty;
        /// <summary>
        /// Just the subfolder name under "InstalledMods"
        /// </summary>
        public string InstallFolder
        {
            get { return installFolder; }
            set
            {
                installFolder = value;
                DoChanged();
            }
        }
        private Guid localIdentifier = Guid.NewGuid();
        public Guid LocalIdentifier
        {
            get { return localIdentifier; }
            set
            {
                localIdentifier = value;
                DoChanged();
            }
        }
        private bool isActive = false;
        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                DoChanged();
            }
        }
        private bool isInstalled = false;
        public bool IsInstalled
        {
            get { return isInstalled; }
            set
            {
                isInstalled = value;
                DoChanged();
            }
        }

        private bool isMission = false;
        public bool IsMission
        {
            get { return isMission; }
            set
            {
                isMission = value;
                DoChanged();
            }
        }
        private int stackOrder = 0;
        /// <summary>
        /// Identifies what position this mod is in the list of activated mods.  Only useful for listing activated mods.  Base Artemis SBS should ALWAYS be 0, and NEVER should multiple different
        /// versions of Artemis SBS be activated!!!!
        /// </summary>
        public int StackOrder
        {
            get { return stackOrder; }
            set
            {
                stackOrder = value;
                DoChanged();
            }
        }
        private string packageFile = string.Empty;
        public string PackageFile
        {
            get
            {
                return packageFile;
            }
            set
            {
                packageFile = value;
                DoChanged();
            }
        }
        public string SaveFile { get; set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public string GetJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.Serialize(this, options);
        }
        public static ModItem? GetModItem(string jsonData)
        {
            try
            {
                return JsonSerializer.Deserialize<ModItem>(jsonData);
            }
            catch (Exception ex)
            { 
                return null;
            }
        }
        public static ModItem? LoadModItem(string file)
        {
            using StreamReader sr = new(file);
            string data = sr.ReadToEnd();
            ModItem? modItem = GetModItem(data);
            return modItem;
        }
        /// <summary>
        /// Make sure "IsArtemisBase" and Version are set before calling this method.
        /// </summary>
        /// <returns></returns>
        public string GetInstallFolder()
        {
            
            if (string.IsNullOrEmpty(InstallFolder))
            {
                if (ModIdentifier == Guid.Empty)
                {
                    if (IsArtemisBase)
                    {
                        InstallFolder = "ArtemisV" + Version;
                    }
                    else
                    {
                        InstallFolder = localIdentifier.ToString();
                    }
                }
                else
                {
                    InstallFolder = ModIdentifier.ToString();
                }
            }
            
            return InstallFolder;
        }
        /// <summary>
        /// Recommend calling GetInstallFolder first.
        /// Is only the name--does not include full path.
        /// </summary>
        /// <returns></returns>
        public string GetSaveFile()
        {
            string file;
            if (string.IsNullOrEmpty(InstallFolder))
            {
                file = GetInstallFolder() + ArtemisManager.SaveFileExtension;
            }
            else
            {
                file = InstallFolder + ArtemisManager.SaveFileExtension;
            }
            return file;
        }
        private string GetFullSavePath()
        {
            if (IsMission)
            {
                return MissionInstallFolder;
            }
            else
            {
                return ModInstallFolder;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">Must be full path, otherwise InstallFolder+SaveFile used.</param>
        public void Save(string file = "")
        {
            if (string.IsNullOrEmpty(SaveFile))
            {
                SaveFile = GetSaveFile();
            }

            if (string.IsNullOrEmpty(file))
            {
                file = Path.Combine(GetFullSavePath(), SaveFile);
            }
            string data = GetJSON();
            ModManager.CreateFolder(Path.Combine(GetFullSavePath(), InstallFolder));
            using StreamWriter sw = new(file);
            sw.WriteLine(data);
        }
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                return Equals(obj as ModItem);
            }
        }
        public bool Equals(ModItem? other)
        {
            if (other != null)
            {
                this.SaveFile = this.GetSaveFile();
                return this.SaveFile.Equals(other.SaveFile);
            }
            else
            {
                return false;
            }
                
        }
        public void Unpack()
        {
            ModManager.CreateFolder(Path.Combine(ModInstallFolder,installFolder));
            using Stream stream = File.OpenRead(Path.Combine(ModManager.ModArchiveFolder, PackageFile));
            Unpack(stream);
        }
        public void Unpack(Stream stream)
        {
            string targetFolder;
            if (IsMission)
            {
                targetFolder = Path.Combine(MissionInstallFolder, installFolder);
            }
            else
            {
                targetFolder = Path.Combine(ModInstallFolder, installFolder);
            }
            ModManager.CreateFolder(targetFolder);
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    reader.WriteEntryToDirectory(targetFolder, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true

                    });
                }
            }
            this.IsInstalled = true;
            this.Save();
        }
        public void StoreAndUnpack(byte[] package)
        {
            ModManager.CreateFolder(ModManager.ModArchiveFolder);
            string packageTarget = Path.Combine(ModManager.ModArchiveFolder, packageFile);
            if (File.Exists(packageTarget))
            {
                File.Delete(packageTarget);
            }
            using (var fs = new FileStream(packageTarget, FileMode.CreateNew))
            {
                fs.Write(package,0, package.Length);
            }
            string installFolder = Path.Combine(ModInstallFolder, GetInstallFolder());
            ModManager.CreateFolder(installFolder);

            using MemoryStream stream = new(package);
            Unpack(stream);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.GetSaveFile());
        }
    }
}