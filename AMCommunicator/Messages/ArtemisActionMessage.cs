using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.AretmisAction)]
    internal class ArtemisActionMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ArtemisActionMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ArtemisActionMessage() : base()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {

        }
        public enum Actions
        {
            StartArtemis,
            StopArtemis,
            ActivateMod,
            DeactivateMod,
            ResetToVanilla
        }
        public void SetAction(Actions action)
        {
            Action = (int)action;
        }
        public Actions GetAction()
        {
            return (Actions)Action;
        }
        [NetworkMessage(Sequence = 4)]
        public int Action { get; set; }
        [NetworkMessage(Sequence = 5)]
        public MessageString ItemIdentifier { get; protected set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.AretmisAction;
            MessageVersion = ThisVersion;
        }
    }
}
