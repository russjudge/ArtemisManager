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
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        internal ClientInfoMessage() : base()
        {
            AppVersion = string.Empty;
            InstalledMods = Array.Empty<string>();
            InstalledMissions= Array.Empty<string>();
            AllDrives = Array.Empty<string>();
            GeneralSettings = string.Empty;
        }
        public ClientInfoMessage(bool isMaster, bool connectOnStart, string[] installedMods,
            string[] installedMissions, bool artemisIsRunning, bool isUsingThisAppControlledArtemis, bool appInStartFolder) : base()
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
                        AppVersion = nm.Version.ToString();
                    }
                }
            }
            ConnectOnstart = connectOnStart;
            InstalledMods = installedMods;
            InstalledMissions = installedMissions;
            ArtemisIsRunning = artemisIsRunning;
            IsUsingThisAppControlledArtemis = isUsingThisAppControlledArtemis;
            AppInStartFolder = appInStartFolder;
           
            var drives = DriveInfo.GetDrives();
            List<string> dd = new();
            foreach (var drive in drives)
            {
                if (drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)[..1]))
                {
                    FreeSpaceOnAppDrive = drive.AvailableFreeSpace;
                }
                if (drive.DriveType == DriveType.Fixed )
                {
                    dd.Add(drive.Name + ","+ drive.DriveType.ToString()+","+drive.TotalSize.ToString() + ","+drive.AvailableFreeSpace);
                }

            }
            AllDrives = dd.ToArray();

            GeneralSettings = string.Empty;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        public bool IsMaster { get; protected set; }

        public string AppVersion { get; protected set; }

        public bool ConnectOnstart { get; protected set; }

        public string[] InstalledMods { get; protected set; }


        //Reserved for future use: Installed missions will be part of phase 2.
        public string[] InstalledMissions { get; protected set; }

        public bool ArtemisIsRunning { get; protected set; }
        //Below is to differentiate from Artemis running from original install and Artemis running from folder controlled by this application.
        //  This is useful to warn of potential issues.
        public bool IsUsingThisAppControlledArtemis { get; protected set; }

        public bool AppInStartFolder { get; protected set; }


        public long FreeSpaceOnAppDrive { get; protected set; }

        public string[] AllDrives { get; protected set; }

        //This is a catch-all.
        public string GeneralSettings { get; protected set; }
    }
}

