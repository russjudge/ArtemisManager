﻿using System;
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
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
        public string GetSaveFile()
        {
            string file;
            if (ModIdentifier == Guid.Empty)
            {
                file = localIdentifier.ToString() + ArtemisManager.SaveFileExtension;
            }
            else
            {
                file = ModIdentifier.ToString() + ArtemisManager.SaveFileExtension;
            }

            file = Path.Combine(ModInstallFolder, file);

            return file;
        }
        public void Save(string file = "")
        {
            string data = GetJSON();
            if (string.IsNullOrEmpty(file)) 
            {
                file = GetSaveFile();
            }
            SaveFile = file;
            using StreamWriter sw = new(file);
            sw.WriteLine(data);
            
        }
    }
}
