using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.ClientInfo)]
    internal class ClientInfoMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ClientInfoMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public ClientInfoMessage(bool isMaster, int appVersion, bool connectOnStart, string[] installedMods,
            string[] archivedMods, string[] installedArchivedVersions, string[] installedMissions, string[] activeMissions,
            int activeArtemisVersion, bool artemisIsRunning, bool isUsingThisAppControlledArtemis, bool appInStartFolder) : base()
        {
            IsMaster= isMaster;
            AppVersion= appVersion;
            ConnectOnstart= connectOnStart;
            InstalledMods = new MessageString(string.Join("|", installedMods));
            ArchivedMods = new MessageString(string.Join("|", archivedMods));
            InstalledArtemisVersions = new MessageString(string.Join("|", installedArchivedVersions));
            InstalledMissions = new MessageString(string.Join("|", installedMissions));
            ActiveMissions = new MessageString(string.Join("|", activeMissions));
            ActiveArtemisVersion = activeArtemisVersion;
            ArtemisIsRunning= artemisIsRunning;
            IsUsingThisAppControlledArtemis= isUsingThisAppControlledArtemis;
            AppInStartFolder= appInStartFolder;
            var assm = Assembly.GetEntryAssembly();
            var location = new FileInfo(assm.Location);
            var drives = DriveInfo.GetDrives();
            List<string> dd = new List<string>();
            foreach (var drive in drives)
            {
                if (drive.Name.StartsWith(location.FullName.Substring(0,1)))
                {
                    FreeSpaceOnAppDrive = drive.AvailableFreeSpace;
                }
                if (drive.DriveType == DriveType.Fixed )
                {
                    dd.Add(drive.Name + ","+ drive.DriveType.ToString()+","+drive.TotalSize.ToString() + ","+drive.AvailableFreeSpace);
                }

            }
            AllDrives = new MessageString(string.Join("|", dd.ToArray()));

            GeneralSettings = new MessageString(string.Empty);
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.ClientInfo;
            MessageVersion = ThisVersion;
        }


        [NetworkMessage(Sequence = 10)]
        public bool IsMaster { get; set; }

        //use negative numbers for the 0.x versions.
        //For versions 1-9, first digit will be major version number.  All rest of digits will be to right of decimal point.
        //This breaks above version 9, so at version 8, figure a new solution as version 9 will NOT use this to handle
        // the possibility of a version 10.  But we're not going that high--so probably nothing to worry about.
        // 0.1 = -1, 0.2 = -2.  1.0 = 1, 1.12 = 112., 2.5 = 25.  This won't work above version 9, but we're not going that high.

        [NetworkMessage(Sequence = 20)]
        public int AppVersion { get; set; }

        [NetworkMessage(Sequence = 30)]
        public bool ConnectOnstart { get; set; }

        [NetworkMessage(Sequence = 40)]
        public MessageString InstalledMods { get; set; }


        //Generally the same as Installed mods, but could have some archived that have since been uninstalled.
        [NetworkMessage(Sequence = 50)]
        public MessageString ArchivedMods { get; set; }

        [NetworkMessage(Sequence = 60)]
        public MessageString InstalledArtemisVersions { get; set; }

        //Reserved for future use: Installed missions will be part of phase 2.
        [NetworkMessage(Sequence = 70)]
        public MessageString InstalledMissions { get; set; }
        //Reserved for future use: Active missions will be part of phase 2.
        [NetworkMessage(Sequence = 80)]
        public MessageString ActiveMissions { get; set; }
        [NetworkMessage(Sequence = 90)]
        public int ActiveArtemisVersion { get; set; }
        [NetworkMessage(Sequence = 100)]
        public bool ArtemisIsRunning { get; set; }
        //Below is to differentiate from Artemis running from original install and Artemis running from folder controlled by this application.
        //  This is useful to warn of potential issues.
        [NetworkMessage(Sequence = 110)]
        public bool IsUsingThisAppControlledArtemis { get; set; }
        [NetworkMessage(Sequence = 120)]
        public bool AppInStartFolder { get; set; }
        [NetworkMessage(Sequence = 130)]
        public long FreeSpaceOnAppDrive { get; set; }

        [NetworkMessage(Sequence = 140)]
        public MessageString AllDrives { get; set; }

        //This is a catch-all.
        [NetworkMessage(Sequence = 400)]
        public MessageString GeneralSettings { get; set; }
    }
}

