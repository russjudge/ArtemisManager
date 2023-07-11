using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Item)]
    internal class ItemMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public ItemMessage() : base()
        {
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
    }
}
