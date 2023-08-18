using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ClientInfoEventArgs : EventArgs
    {
        internal ClientInfoEventArgs(ClientInfoMessage message)
        {
            if (message.Source != null)
            {
                Source = IPAddress.Parse(message.Source);
            }
            IsMaster= message.IsMaster;
            AppVersion = message.AppVersion;
            ConnectOnStart = message.ConnectOnstart;
            InstalledMods = message.InstalledMods;
            InstalledMissions = message.InstalledMissions;
            ArtemisIsRunning= message.ArtemisIsRunning;
            IsUsingThisAppControlledArtemis = message.IsUsingThisAppControlledArtemis;
            AppInStartFolder = message.AppInStartFolder;
            Drives = message.Drives;
            IsMainScreenServer = message.IsMainScreenServer;
            GeneralSettings = message.GeneralSettings;
        }
        public IPAddress? Source { get; private set; }
        public bool IsMaster { get; private set; }
        public string AppVersion { get; private set; }
        public bool ConnectOnStart { get; private set; }
        public string[] InstalledMods { get; private set; }
        public string[] InstalledMissions { get; private set; }
        public bool ArtemisIsRunning { get; private set; }
        public bool IsUsingThisAppControlledArtemis { get; private set; }
        public bool AppInStartFolder { get; private set; }
        [Obsolete]
        public long FreeSpaceOnAppSrive { get; private set; } = 0;
        [Obsolete]
        public string[] AllDrives { get; private set; } = Array.Empty<string>();
        public DriveData[] Drives { get; private set; }
        public bool IsMainScreenServer { get; private set; }
        public string GeneralSettings { get; private set; }
    }
}
