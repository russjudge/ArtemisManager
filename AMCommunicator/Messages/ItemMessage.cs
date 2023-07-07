using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Item)]
    internal class ItemMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ItemMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ItemMessage() : base()
        {
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.Item;
            MessageVersion = ThisVersion;
        }
    }
}
