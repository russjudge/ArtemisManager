﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.AretmisAction)]
    internal class ArtemisActionMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public ArtemisActionMessage() : base()
        {
            ItemIdentifier = string.Empty;
        }
        public ArtemisActionMessage(ArtemisActions action, string itemIdentifier)
        {
            Action = action;
            ItemIdentifier = itemIdentifier;

        }
        
        
        public ArtemisActions Action { get; protected set; }
        public string ItemIdentifier { get; protected set; }

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
