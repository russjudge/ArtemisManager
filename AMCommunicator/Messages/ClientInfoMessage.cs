using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.ClientInfo)]
    internal class ClientInfoMessage : NetworkMessage
    {
        public const short ThisVersion = 1;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        //From version 0 to version 1:
        // Added Drives array.  This may cause a crash loading to an old version.  Not sure.  Testing may be necessary.
        //  Code in current state refuses to load the class over network and will alert user of need to apply update to Artemis Manager.
        internal ClientInfoMessage() : base()
        {
            AppVersion = string.Empty;
            InstalledMods = Array.Empty<string>();
            InstalledMissions= Array.Empty<string>();
            Drives = Array.Empty<DriveData>();
            GeneralSettings = string.Empty;
        }
        public ClientInfoMessage(
            bool isMaster,
            bool connectOnStart,
            string[] installedMods,
            string[] installedMissions,
            bool artemisIsRunning,
            bool isUsingThisAppControlledArtemis,
            bool appInStartFolder,
            bool isMainScreenServer = false) : base()
        {
            IsMaster = isMaster;

            AppVersion = string.Empty;
            var assm = Assembly.GetEntryAssembly();
            if (assm != null)
            {
                var nm = assm.GetName();
                if (nm != null)
                {
                    if (nm.Version != null)
                    {
                        AppVersion =  nm.Version.ToString();
                    }
                }
            }
            ConnectOnstart = connectOnStart;
            InstalledMods = installedMods;
            InstalledMissions = installedMissions;
            ArtemisIsRunning = artemisIsRunning;
            IsUsingThisAppControlledArtemis = isUsingThisAppControlledArtemis;
            AppInStartFolder = appInStartFolder;
            Drives = DriveData.GetDriveData();

            List<string> dd = new();
            foreach (var drive in Drives)
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    dd.Add(drive.Name + ","+ drive.DriveType.ToString()+","+drive.TotalSize.ToString() + ","+drive.FreeSpace);
                }
            }
            IsMainScreenServer = isMainScreenServer;
            GeneralSettings = string.Empty;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public bool IsMaster { get; set; }

        public string AppVersion { get; set; }

        public bool ConnectOnstart { get; set; }

        public string[] InstalledMods { get; set; }


        //Reserved for future use: Installed missions will be part of phase 2.
        public string[] InstalledMissions { get; set; }

        public bool ArtemisIsRunning { get; set; }
        //Below is to differentiate from Artemis running from original install and Artemis running from folder controlled by this application.
        //  This is useful to warn of potential issues.
        public bool IsUsingThisAppControlledArtemis { get; set; }

        public bool AppInStartFolder { get; set; }

        [Obsolete]
        public long FreeSpaceOnAppDrive { get; set; } = 0;

        [Obsolete]
        public string[] AllDrives { get; set; } = Array.Empty<string>();
        public DriveData[] Drives { get; set; }
        public bool IsMainScreenServer { get; set; }

        //This is a catch-all.
        public string GeneralSettings { get; set; }
    }
}

