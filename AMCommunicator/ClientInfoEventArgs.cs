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
            Source = message.Source;
            IsMaster= message.IsMaster;
            AppVersion = message.AppVersion;
            ConnectOnStart = message.ConnectOnstart;
            InstalledMods = message.InstalledMods;
            InstalledMissions = message.InstalledMissions;
            ArtemisIsRunning= message.ArtemisIsRunning;
            IsUsingThisAppContorlledArtemis = message.IsUsingThisAppControlledArtemis;
            AppInStartFolder = message.AppInStartFolder;
            FreeSpaceOnAppSrive = message.FreeSpaceOnAppDrive;

            AllDrives = message.AllDrives;
            GeneralSettings = message.GeneralSettings;
        }
        public IPAddress? Source { get; private set; }
        public bool IsMaster { get; private set; }
        public string AppVersion { get; private set; }
        public bool ConnectOnStart { get; private set; }
        public string[] InstalledMods { get; private set; }
        public string[] InstalledMissions { get; private set; }
        public bool ArtemisIsRunning { get; private set; }
        public bool IsUsingThisAppContorlledArtemis { get; private set; }
        public bool AppInStartFolder { get; private set; }
        public long FreeSpaceOnAppSrive { get; private set; }
        public string[] AllDrives { get; private set; }
        public string GeneralSettings { get; private set; }
    }
}
