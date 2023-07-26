using AMCommunicator;
using ArtemisManagerUI.Properties;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArtemisManagerUI
{
    public class SettingsAction : INotifyPropertyChanged
    {

        public static readonly string[] SynchronizableSettings = new string[] {
            nameof(Settings.ConnectOnStart),
            nameof(Settings.ListeningPort),
            nameof(Settings.NetworkPassword)
        };
        
        public bool SynchronizeEnabled { get; set; } = true;
        public static bool IsSynchronizable(string name)
        {
            return SynchronizableSettings.Contains(name);
        }
        public static void Touch()
        {

        }
            
        static SettingsAction()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            Network.ConnectionPort = Current.ListeningPort;
            Network.Password = Current.NetworkPassword;
        }
        private SettingsAction()
        {
           
        }
       
        public event PropertyChangedEventHandler? PropertyChanged;
        void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            IsChanged = true;
            var prop = typeof(SettingsAction).GetProperty(property);
            if (prop != null)
            {
                var value = prop.GetValue(this);
                string? val;
                if (value == null)
                {
                    val = string.Empty;
                }
                else
                {
                    val = value.ToString();
                }
                DoSynchronizeAction(property, val);
            }
            
        }
        private bool isChanged = false;
        public bool IsChanged
        {
            get
            {
                return isChanged;
            }
            private set
            {
                isChanged = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChanged)));
            }
        }
        public void Save()
        {
            Properties.Settings.Default.Save();
            IsChanged = false;
            SynchronizeEnabled = true;
        }

        /// <summary>
        /// For changing a setting from an external source
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void ChangeSetting(string name, string value)
        {
            var property = typeof(Settings).GetProperty(name);

            //var property = this.GetType().GetProperty(name);
            if (property != null)
            {
                if (property.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(value, out bool boolvalue))
                    {
                        property.SetValue(Properties.Settings.Default, boolvalue);
                        DoChanged(name);
                    }
                }
                else if (property.PropertyType == typeof(int))
                {
                    if (int.TryParse(value, out int intvalue))
                    {
                        property.SetValue(Properties.Settings.Default, intvalue);
                        DoChanged(name);
                    }

                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(Properties.Settings.Default, value);
                    DoChanged(name);
                }
                DoSynchronizeAction(name, value);
            }
        }
        private void DoSynchronizeAction(string name, string? value)
        {
            if (SynchronizeEnabled && IsSynchronizable(name))
            {
                //var result = TakeAction.DoChangeSetting(name, value);
                if (TakeAction.DoChangeSetting(name, value))
                {
                    this.Save();
                }
                else
                {
                    Settings.Default.Reload();
                    Network.ConnectionPort = ListeningPort;
                    Network.Password = NetworkPassword;
                }
            }
        }
        public bool ConnectOnStart
        {
            get
            {
                return Properties.Settings.Default.ConnectOnStart;
            }
            set
            {
                Properties.Settings.Default.ConnectOnStart = value;
                DoChanged();
            }
        }
        public int ListeningPort
        {
            get
            {
                return Properties.Settings.Default.ListeningPort;
            }
            set
            {

                Properties.Settings.Default.ListeningPort = value;
                Network.ConnectionPort = value;
                DoChanged();
            }
        }
        public bool IsMaster
        {
            get
            {
                return Properties.Settings.Default.IsMaster;
            }
            set
            {
                Properties.Settings.Default.IsMaster = value;
                DoChanged();
            }
        }
        public string NetworkPassword
        {
            get
            {
                return Properties.Settings.Default.NetworkPassword;
            }
            set
            {
                Properties.Settings.Default.NetworkPassword = value;
                Network.Password = value;
                DoChanged();
            }
        }

        private static SettingsAction instance = new();
       
        public static SettingsAction Current
        {
            get
            {
                return instance;
            }
        }

    }
}
