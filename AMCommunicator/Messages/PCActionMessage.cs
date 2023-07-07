using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    [NetworkMessageCommand(MessageCommand.PCAction)]
    internal class PCActionMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public PCActionMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public PCActionMessage() : base()
        {

        }
        public PCActionMessage(PCActions action, bool force) : base()
        {
            Force = force;
            SetAction(action);
        }

        public void SetAction(PCActions action)
        {
            Action = (short)action;
        }
        public PCActions GetAction()
        {
            return (PCActions)Action;
        }
        [NetworkMessage(Sequence = 4)]
        public short Action { get; private set; }
        public bool Force { get; private set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.PCAction;
            MessageVersion = ThisVersion;
        }
    }
}
