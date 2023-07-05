using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{

    [NetworkMessageCommand(MessageCommand.PCAction)]
    public class PCActionMessage : NetworkMessageAbstract
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PCActionMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public PCActionMessage() : base()
        {
            
        }
        public enum Actions : short
        {
            CloseApp,
            ShutdownPC,
            RestartPC,
            CheckForUpdate,
            DisconnectThisConnection
        }
        public void SetAction(Actions action)
        {
            Action = (short)action;
        }
        public Actions GetAction()
        {
            return (Actions)Action;
        }
        [NetworkMessage(Sequence = 4)]
        public short Action { get; set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.PCAction;
        }
    }
}
