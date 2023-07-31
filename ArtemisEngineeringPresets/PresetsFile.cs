using AMCommunicator;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Media;

namespace ArtemisEngineeringPresets
{
    public class PresetsFile : DependencyObject, ISendableStringFile
    {
        public const int MaxPresets = 10;
        public const int MaxStations = 8;
        public const byte HeaderByte = 0xfe;
        public PresetsFile()
        {
            Presets = new Preset[MaxPresets];
            for (int i = 0; i < MaxPresets; i++)
            {
                for (int j = 0; j < MaxStations; j++)
                {
                    Presets[i] = new Preset();
                    Presets[i].SystemLevels[j] = new();
                }
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
        public string SaveFile { get; set; } = string.Empty;
        private void LoadPresets(string file)
        {
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
                        for (int j = 0; j < MaxStations; j++)
                        {
                            energyLevels.Add((int)Math.Round(br.ReadSingle() * 300));
                        }
                        for (int j = 0; j < MaxStations; j++)
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
            Presets = new Preset[MaxPresets];
            for (int i = 0; i < MaxPresets; i++)
            {
                Presets[i] = workPresets[i];
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
                if (Presets == null || Presets.Length != MaxPresets)
                {
                    throw new InvalidPresetFileException("There must be EXACTLY 10 presets");
                }
                else
                {
                    using BinaryWriter bw = new(File.Open(SaveFile, FileMode.Create, FileAccess.Write));
                    bw.Write(new byte[] { HeaderByte, HeaderByte });
                    for (int j = 0; j < MaxPresets; j++)
                    {
                        for (int i = 0; i < MaxStations; i++)
                        {
                            bw.Write((float)Presets[j].SystemLevels[i].EnergyLevel / 300);
                        }
                        for (int i = 0; i < MaxStations; i++)
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
        public static readonly DependencyProperty PresetsProperty =
         DependencyProperty.Register(nameof(Presets), typeof(Preset[]),
        typeof(PresetsFile));

        public Preset[] Presets
        {
            get
            {
                return (Preset[])this.GetValue(PresetsProperty);

            }
            private set
            {
                this.SetValue(PresetsProperty, value);
            }
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
                WriteIndented = false
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}
