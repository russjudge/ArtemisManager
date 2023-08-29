using AMCommunicator;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ArtemisManagerAction.ArtemisEngineeringPresets
{
    [JsonSerializable(typeof(PresetsFile))]
    public class PresetsFile : INotifyPropertyChanged, ISendableStringFile
    {
        public const int MaxPresets = 10;
        public const byte HeaderByte = 0xfe;
        public PresetsFile()
        {
            
        }
        public void Initialize()
        {
            for (int i = 0; i < MaxPresets; i++)
            {
                Presets.Add(new Preset());
                Presets[i].Index = i + 1;
            }
        }

        public PresetsFile(string file)
        {
            LoadPresets(file);
        }

        public bool HasChanges()
        {
            foreach (var preset in Presets)
            {
                if (preset.Changed)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// The full path
        /// </summary>
        public string SaveFile { get; set; } = string.Empty;
        public string PackageFile
        {
            get
            {
                return SaveFile;
            }
        }
        private void LoadPresets(string file)
        {
            Presets.Clear();
            List<Preset> workPresets = new();
            if (File.Exists(file))
            {
                using (BinaryReader br = new(File.Open(file, FileMode.Open, FileAccess.Read)))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (br.ReadByte() != HeaderByte)
                        {
                            throw new InvalidPresetFileException("Header \"0xfefe\" missing!");
                        }
                    }
                    for (int i = 0; i < MaxPresets; i++)
                    {
                        List<int> energyLevels = new();
                        List<int> coolantLevels = new();
                        for (int j = 0; j < Preset.MaxStations; j++)
                        {
                            energyLevels.Add((int)Math.Round(br.ReadSingle() * 300));
                        }
                        for (int j = 0; j < Preset.MaxStations; j++)
                        {
                            coolantLevels.Add(Convert.ToInt32(br.ReadByte()));
                        }
                        Preset p = new(energyLevels, coolantLevels, i + 1);

                        workPresets.Add(p);
                    }
                }

                if (workPresets.Count != MaxPresets)
                {
                    throw new InvalidPresetFileException("Preset file must have EXACTLY " + MaxPresets.ToString() + " presets!");
                }
            }
            else
            {
                for (int i = 0; i < MaxPresets; i++)
                {
                    Preset p = new();
                    workPresets.Add(p);
                }
            }
            
            for (int i = 0; i < MaxPresets; i++)
            {
                Presets.Add(workPresets[i]);
            }
            SaveFile = file;
        }
        public void Delete()
        {
            if (!string.IsNullOrEmpty(SaveFile) && File.Exists(SaveFile))
            {
                File.Delete(SaveFile);
            }
        }
        public void Save()
        {
            if (string.IsNullOrEmpty(SaveFile))
            {
                throw new NullReferenceException("SaveFile is null");
            }
            else
            {
                if (Presets == null || Presets.Count != MaxPresets)
                {
                    throw new InvalidPresetFileException("There must be EXACTLY 10 presets");
                }
                else
                {
                    using BinaryWriter bw = new(File.Open(SaveFile, FileMode.Create, FileAccess.Write));
                    bw.Write(new byte[] { HeaderByte, HeaderByte });
                    for (int j = 0; j < MaxPresets; j++)
                    {
                        for (int i = 0; i < Preset.MaxStations; i++)
                        {
                            bw.Write((float)Presets[j].SystemLevels[i].EnergyLevel / 300);
                        }
                        for (int i = 0; i < Preset.MaxStations; i++)
                        {
                            byte b = (byte)Presets[j].SystemLevels[i].CoolantLevel;
                            bw.Write(b);
                        }
                    }
                }
            }
        }
        public void Save(string file)
        {
            SaveFile = file;
            Save();
        }
       

        public event PropertyChangedEventHandler? PropertyChanged;
        
        [JsonInclude()]
        public ObservableCollection<Preset> Presets { get; set; } = new ObservableCollection<Preset>();

        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public SendableStringPackageFile FileType
        {
            get
            {
                return SendableStringPackageFile.EngineeringPreset;
            }
        }

        public string GetSerializedString()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}