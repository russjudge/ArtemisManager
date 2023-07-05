using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    [NetworkMessageCommand(MessageCommand.RequestItem)]
    public class RequestItemMessage : NetworkMessageAbstract
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public RequestItemMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
      
        public RequestItemMessage() : base()
        {
            ItemIdentifier = new MessageString();
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.RequestItem;
        }

        [NetworkMessage(Sequence = 4)]
        public MessageString ItemIdentifier { get; set; }
    }
}
