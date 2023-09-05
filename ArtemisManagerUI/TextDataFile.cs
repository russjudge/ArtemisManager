using AMCommunicator;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class TextDataFile : ISendableStringFile, INotifyPropertyChanged
    {
        public TextDataFile(SendableStringPackageFile fileType, string name, string saveFile)
        {
            FileType = fileType;
            Name = name;
            if (Name.Contains('/') || Name.Contains('\\'))
            {
                Name = new System.IO.FileInfo(Name).Name;
            }
            if (Name.Contains('.'))
            {
                Name = Name.Substring(0, Name.Length - 4);
            }
            OriginalName = Name;
            SaveFile = saveFile;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        private bool isEditMode = false;
        public bool IsEditMode
        {
            get { return isEditMode; }
            set
            {
                isEditMode = value;
                DoChanged();
            }
        }
        public SendableStringPackageFile FileType { get; set; }


        private string saveFile = string.Empty;
        public string SaveFile
        { 
            get
            {
                return saveFile;
            }
            set
            {
                saveFile = value;
                DoChanged();
            }
        }

        public string PackageFile
        {
            get
            {
                return SaveFile;
            }
            set
            {
                SaveFile = value;
                DoChanged();
            }
        }
        private string data = string.Empty;
        public string Data
        {
            get
            {
                return data;

            }
            set
            {
                data = value;
                DoChanged();
            }
        }
        private string name = string.Empty;
        public string Name
        {
            get
            {
                return name;

            }
            set
            {
                name = value;
                DoChanged();
            }
        }
        private string originalname = string.Empty;
        public string OriginalName
        {
            get
            {
                return originalname;

            }
            set
            {
                originalname = value;
                DoChanged();
            }
        }
        public string GetSerializedString()
        {
            return Data;
        }
        public string GetJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
