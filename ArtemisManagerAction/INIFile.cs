using AMCommunicator;
using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class INIFile : INotifyPropertyChanged, ISendableJsonFile
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        protected void LoadFile(string file)
        {
            List<string> settingNames = new();


            SaveFile = file;
            using StreamReader sr = new(file);
            ArtemisINISetting setting = new();
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                setting.ProcessLine(line);
                if (!string.IsNullOrEmpty(setting.SettingName))
                {
                    bool okaytoAdd = true;
                    if (settingNames.Contains(setting.SettingName.ToUpperInvariant()))
                    {
                        okaytoAdd = false;
                        if (!setting.UsingDefault)
                        {

                            ArtemisINISetting? settingToRemove = null;
                            foreach (var s in settingList)
                            {
                                if (s.SettingName == setting.SettingName)
                                {
                                    settingToRemove = s;
                                    break;
                                }
                            }
                            if (settingToRemove != null)
                            {
                                okaytoAdd = true;
                                settingList.Remove(settingToRemove);
                            }
                        }
                    }
                    if (okaytoAdd)
                    {
                        settingList.Add(setting);
                        settingNames.Add(setting.SettingName.ToUpperInvariant());
                    }
                    setting = new();
                }
            }
            if (setting.CommentLines.Count > 0)
            {
                settingList.Add(setting);
            }
            foreach (var item in settingList)
            {
                if (!string.IsNullOrEmpty(item.SettingName))
                {
                    Settings.Add(item.SettingName.ToUpperInvariant(), item);
                }
            }

        }
        public void Save(string filename)
        {
            SaveFile = filename;
            Save();
        }
        public void Save()
        {
            if (!string.IsNullOrEmpty(SaveFile))
            {
                if (File.Exists(SaveFile))
                {
                    File.Delete(SaveFile);
                }
                using StreamWriter sw = new(SaveFile);
                sw.WriteLine(ToString());
            }
            else
            {
                throw new InvalidOperationException("Cannot save: save file not set.");
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            Dictionary<int, ArtemisINISetting> items = new();
            foreach (var item in settingList)
            {
                var prop = this.GetType().GetProperty(item.SettingName);
                if (prop != null)
                {
                    var attr = prop.GetCustomAttribute<INISettingAttribute>();
                    attr ??= prop.GetCustomAttribute<LocalINISettingAttribute>();
                    if (attr != null)
                    {
                        items.Add(attr.Sequence, item);
                    }
                }
            }
            foreach (var key in items.Keys)
            {
                sb.AppendLine(items[key].ToString());
            }
            return sb.ToString();
        }
        List<ArtemisINISetting> settingList = new();
        Dictionary<string, ArtemisINISetting> _settings = new();
        public Dictionary<string, ArtemisINISetting> Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                DoChanged();
            }
        }
        private string _saveFile = string.Empty;
        public string SaveFile
        {
            get
            {
                return _saveFile;
            }
            set
            {
                _saveFile = value;
                DoChanged();
            }
        }
        protected void SetSetting(ArtemisINISetting value, [CallerMemberName] string? key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                EnsureSettingExists(key);
                Settings[key.ToUpperInvariant()] = value;
                DoChanged(key);
            }
            else
            {
                throw new NullReferenceException("Null key value");
            }
        }
        protected ArtemisINISetting EnsureSettingExists([CallerMemberName] string? key = null)
        {
            if (!string.IsNullOrEmpty(key) && !Settings.ContainsKey(key.ToUpperInvariant()))
            {
                ArtemisINISetting defaultValue = new(key, string.Empty, true);
                Settings.Add(key.ToUpperInvariant(), defaultValue);
                settingList.Add(defaultValue);
                DoChanged(key);
            }
            if (!string.IsNullOrEmpty(key))
            {
                return Settings[key.ToUpperInvariant()];
            }
            else
            {
                throw new NullReferenceException();
            }
        }
        protected void DoSet(int value, [CallerMemberName] string key = "")
        {
            DoChanged(key);
            EnsureSettingExists(key);
            Settings[key].SetValue(value);
        }
        protected int GetInt([CallerMemberName] string key = "")
        {
            EnsureSettingExists(key);
            return Settings[key].GetIntValue();
        }
        protected void DoSet(string value, [CallerMemberName] string key = "")
        {
            DoChanged(key);
            EnsureSettingExists(key);
            Settings[key].SetValue(value);
        }
        protected string GetString([CallerMemberName] string key = "")
        {
            EnsureSettingExists(key);
            return Settings[key].FileValue;
        }
        protected void DoSet(double value, [CallerMemberName] string key = "")
        {
            DoChanged(key);
            EnsureSettingExists(key);
            Settings[key].SetValue(value);
        }
        protected double GetDouble([CallerMemberName] string key = "")
        {
            EnsureSettingExists(key);
            return Settings[key].GetDoubleValue();
        }
        protected void DoSet(bool value, [CallerMemberName] string key = "")
        {
            DoChanged(key);
            EnsureSettingExists(key);
            Settings[key].SetValue(value);
        }
        protected bool GetBool([CallerMemberName] string key = "")
        {
            EnsureSettingExists(key);
            return Settings[key].GetBoolValue();
        }
        public virtual JsonPackageFile FileType
        {
            get
            {
                return JsonPackageFile.Generic;
            }
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
