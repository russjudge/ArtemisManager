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
        public TextDataFile(SendableStringPackageFile fileType, string name)
        {
            FileType = fileType;
            Name = name;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public SendableStringPackageFile FileType { get; set; }


        public string SaveFile { get; set; } = string.Empty;

        public string PackageFile
        {
            get
            {
                return SaveFile;
            }
            set
            {
                SaveFile = value;
            }
        }
        public string data = string.Empty;
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
        public string name = string.Empty;
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

        public string GetSerializedString()
        {
            return GetJSON();
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
