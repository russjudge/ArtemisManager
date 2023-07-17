﻿using SharpCompress.Common;
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
        public readonly static string ActivatedFolder = Path.Combine(ModManager.DataFolder, "Activated");
        public ModItem()
        {
            SaveFile = string.Empty;
        }
        /// <summary>
        /// Creates a copy ModItem that is activated.
        /// </summary>
        public ModItem Activate()
        {
            ModItem item = new();
            this.IsActive = true;
            this.Save();
            item.Key = Key;
            item.ModIdentifier = ModIdentifier;

            item.Name = Name;
            item.Description = Description;
            item.Version= Version;
            item.RequiredArtemisVersion= RequiredArtemisVersion;
            item.Author= Author;
            item.IsArtemisBase= IsArtemisBase;
            item.localIdentifier= LocalIdentifier;
            item.ReleaseDate= ReleaseDate;
            item.isActive = true;
            item.InstallFolder = InstallFolder;
            item.PackageFile = PackageFile;
            FileInfo f = new(SaveFile);
            item.Save(Path.Combine(ActivatedFolder, f.Name));
            ModManager.CopyFolder(InstallFolder, Path.Combine(ActivatedFolder, f.Name.Substring(0, f.Name.Length - 4)));
            return item;
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
        private DateTime releaseDate = DateTime.MinValue;
        public DateTime ReleaseDate
        {
            get { return releaseDate; }
            set
            {
                releaseDate = value;
                DoChanged();
            }
        }
        private string? requiredAretmisVersion ="2.8";
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
        public string SaveFile { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public string GetJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
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
        public void Save(string file = "")
        {
            string data = GetJSON();
            if (string.IsNullOrEmpty(SaveFile))
            {
                if (string.IsNullOrEmpty(file))
                {
                    file = GetSaveFile();
                }
                SaveFile = file;
            }
            ModManager.CreateFolder(InstallFolder);
            using StreamWriter sw = new(SaveFile);
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
            ModManager.CreateFolder(installFolder);
            using Stream stream = File.OpenRead(Path.Combine(ModManager.ModArchiveFolder, PackageFile));
            Unpack(stream);
        }
        public void Unpack(Stream stream)
        {
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    using EntryStream entryStream = reader.OpenEntryStream();
                    reader.WriteEntryToDirectory(installFolder, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
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