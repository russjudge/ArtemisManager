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

        public ClientInfoMessage(byte[] data) : base(data) 
        {
            if (AllDrives == null)
            {
                AllDrives = new MessageString(string.Empty);
            }
            if (GeneralSettings == null)
            {
                GeneralSettings = new MessageString(string.Empty);
            }
            if (AppVersion == null)
            {
                AppVersion = new MessageString(string.Empty);
            }
            if (InstalledMissions == null)
            {
                InstalledMissions = new MessageString(string.Empty);
            }
            if (InstalledMods == null)
            {
                InstalledMods = new MessageString(string.Empty);
            }
        }


        public ClientInfoMessage(bool isMaster, bool connectOnStart, string[] installedMods,
            string[] installedMissions, bool artemisIsRunning, bool isUsingThisAppControlledArtemis, bool appInStartFolder) : base()
        {
            IsMaster= isMaster;

            AppVersion = new MessageString(Assembly.GetEntryAssembly().GetName().Version.ToString());
            ConnectOnstart= connectOnStart;
            InstalledMods = new MessageString(string.Join("|", installedMods));
            InstalledMissions = new MessageString(string.Join("|", installedMissions));
            ArtemisIsRunning= artemisIsRunning;
            IsUsingThisAppControlledArtemis= isUsingThisAppControlledArtemis;
            AppInStartFolder= appInStartFolder;
            var assm = Assembly.GetEntryAssembly();
            var location = new FileInfo(assm.Location);
            var drives = DriveInfo.GetDrives();
            List<string> dd = new List<string>();
            foreach (var drive in drives)
            {
                if (drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Substring(0,1)))
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

        
        [NetworkMessage(Sequence = 20)]
        public MessageString AppVersion { get; set; }

        [NetworkMessage(Sequence = 30)]
        public bool ConnectOnstart { get; set; }

        [NetworkMessage(Sequence = 40)]
        public MessageString InstalledMods { get; set; }


        //Reserved for future use: Installed missions will be part of phase 2.
        [NetworkMessage(Sequence = 70)]
        public MessageString InstalledMissions { get; set; }
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

