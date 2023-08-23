using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.ArtemisAction)]
    internal class ArtemisActionMessage : NetworkMessage
    {
        public const short ThisVersion = 1;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public ArtemisActionMessage() : base()
        {
            ItemIdentifier = string.Empty;
            Mod = string.Empty;
            MissionScript = string.Empty;
        }
        public ArtemisActionMessage(ArtemisActions action, Guid itemIdentifier, string mod, string saveName = "")
        {
            Action = action;
            ItemIdentifier = itemIdentifier.ToString();
            Mod = mod;
            MissionScript = string.Empty;
            SaveName= saveName;
        }
        
        
        public ArtemisActions Action { get; set; }
        public string ItemIdentifier { get; set; }

        
        public string Mod { get; set; }
        public string MissionScript { get; set; }

        public string SaveName { get; set; } = string.Empty;

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
