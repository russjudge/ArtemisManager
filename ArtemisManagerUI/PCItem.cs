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
            InstalledMods = new ObservableCollection<ModItem>();
            InstalledMissions = new ObservableCollection<ModItem>();
            AllDrives = new ObservableCollection<string>();
        }
        public void LoadClientInfoData(ClientInfoEventArgs info)
        {
            InstalledMods.Clear();
            InstalledMissions.Clear();
            AllDrives.Clear();
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
            FreeSpaceOnAppDrive = info.FreeSpaceOnAppSrive;
            foreach(var item in info.AllDrives)
            {
                AllDrives.Add(item);
            }
            GeneralSettings= info.GeneralSettings;
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

        private bool connectOnStart = false;
        public bool ConnectOnstart
        {
            get { return connectOnStart; }
            set
            {
                connectOnStart = value;
                DoChanged();
            }
        }

        
        public ObservableCollection<ModItem> InstalledMods { get; private set; }
        


        public ObservableCollection<ModItem> InstalledMissions {get; private set;}

        private bool artemisIsRunning = false;

        public bool ArtemisIsRunning
        {
            get { return artemisIsRunning; }
            set
            {
                artemisIsRunning = value;
                DoChanged();
            }
        }

        private bool isUsingThisAppControlledArtemis = false;
        public bool IsUsingThisAppControlledArtemis
        {
            get { return isUsingThisAppControlledArtemis; }
            set
            {
                isUsingThisAppControlledArtemis = value;
                DoChanged();
            }
        }

        private bool appInStartFolder = false;
        public bool AppInStartFolder
        {
            get { return appInStartFolder; }
            set
            {
                appInStartFolder = value;
                DoChanged();
            }
        }
        private long freeSpaceOnAppDrive = 0;
        public long FreeSpaceOnAppDrive
        {
            get { return freeSpaceOnAppDrive; }
            set
            {
                freeSpaceOnAppDrive = value;
                DoChanged();
            }
        }

        public ObservableCollection<string> AllDrives{get; private set;}


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
