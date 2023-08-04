using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtemisManagerUI
{
    public class PCItem : INotifyPropertyChanged
    {
        public PCItem(string hostname, IPAddress? ip)
        {
            Hostname = hostname;
            IP = ip;
            IsRemote = !TakeAction.IsLoopback(IP);
            InstalledMods = new ObservableCollection<ModItem>();
            InstalledMissions = new ObservableCollection<ModItem>();
            Drives = new();
        }
        public void LoadClientInfoData(ClientInfoEventArgs info)
        {
            IP = info.Source;
            IsRemote = !TakeAction.IsLoopback(IP);
            InstalledMods.Clear();
            InstalledMissions.Clear();
            Drives.Clear();
            IsMaster = info.IsMaster;
            AppVersion= info.AppVersion;
            ConnectOnstart = info.ConnectOnStart;
            foreach (var item in info.InstalledMods)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var mod = ModItem.GetModItem(item);
                    if (mod != null)
                    {
                        InstalledMods.Add(mod);
                    }
                }
            }

            foreach (var item in info.InstalledMissions)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    var mod = ModItem.GetModItem(item);
                    if (mod != null)
                    {
                        InstalledMissions.Add(mod);
                    }
                }
            }
            ArtemisIsRunning = info.ArtemisIsRunning;
            IsUsingThisAppControlledArtemis = info.IsUsingThisAppControlledArtemis;
            AppInStartFolder= info.AppInStartFolder;
            
            GeneralSettings= info.GeneralSettings;
            TakeAction.SetAllConnectionsInfo();
        }
        public string Hostname { get; private set; }
        public IPAddress? IP { get; private set; }


        private bool isMaster = false;
        public bool IsMaster
        {
            get { return isMaster; }
            set
            {
                isMaster = value;
                DoChanged();
            }
        }

        private bool isRemote = false;
        public bool IsRemote
        {
            get { return isRemote; }
            set
            {
                isRemote = value;
                DoChanged();
            }
        }

        private string appVersion = string.Empty;
        public string AppVersion
        {
            get { return appVersion; }
            set
            {
                appVersion = value;
                DoChanged();
            }
        }

        private bool? connectOnStart = false;
        public bool? ConnectOnstart
        {
            get { return connectOnStart; }
            set
            {
                connectOnStart = value;
                DoChanged();
            }
        }

        
        public ObservableCollection<ModItem> InstalledMods { get;  set; }
        


        public ObservableCollection<ModItem> InstalledMissions {get;  set;}

        private bool? artemisIsRunning = false;

        public bool? ArtemisIsRunning
        {
            get { return artemisIsRunning; }
            set
            {
                artemisIsRunning = value;
                DoChanged();
            }
        }

        private bool? isUsingThisAppControlledArtemis = false;
        public bool? IsUsingThisAppControlledArtemis
        {
            get { return isUsingThisAppControlledArtemis; }
            set
            {
                isUsingThisAppControlledArtemis = value;
                DoChanged();
            }
        }

        private bool? appInStartFolder = false;
        public bool? AppInStartFolder
        {
            get { return appInStartFolder; }
            set
            {
                appInStartFolder = value;
                DoChanged();
            }
        }

        public ObservableCollection<DriveData> Drives { get; set; }


        //This is a catch-all.
        private string generalSettings = string.Empty;
        public string GeneralSettings
        {
            get { return generalSettings; }
            set
            {
                generalSettings = value;
                DoChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
