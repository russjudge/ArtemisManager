using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.PCAction)]
    internal class PCActionMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public PCActionMessage() : base()
        {

        }
        public PCActionMessage(PCActions action, bool force) : base()
        {
            Force = force;
            Action = action;
        }

        public PCActions Action { get; protected set; }
        public bool Force { get; protected set; }

        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
