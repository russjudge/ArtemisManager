using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    [NetworkMessageCommand(MessageCommand.AretmisAction)]
    public class ArtemisActionMessage : NetworkMessageAbstract
    {
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
        public MessageString ItemIdentifier { get; set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.AretmisAction;
        }
    }
}
