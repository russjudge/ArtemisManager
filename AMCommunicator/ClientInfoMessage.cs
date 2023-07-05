using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{

    [NetworkMessageCommand(MessageCommand.ClientInfo)]
    public class ClientInfoMessage : NetworkMessageAbstract
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ClientInfoMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        public ClientInfoMessage() : base()
        {
           
        }
        protected override void SetCommand()
        {
            Command = MessageCommand.ClientInfo;
        }


        [NetworkMessage(Sequence = 5)]
        public bool IsMaster { get; set; }
    }
}
