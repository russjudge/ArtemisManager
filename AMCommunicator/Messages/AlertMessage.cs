using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Alert)]
    internal class AlertMessage : NetworkMessageAbstract
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AlertMessage(byte[] data) : base(data) { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public AlertMessage(AlertItems alert, string relatedData ) : base()
        {
            SetAlertItem(alert);
            RelatedData = new MessageString(relatedData);
        }
      
        public AlertItems GetAlertItem()
        {
            return (AlertItems)AlertItem;
        }
        public void SetAlertItem(AlertItems alertItem)
        {
            AlertItem = (short)alertItem;
        }


        //WARNING!!! There is no version check on this NetworkMessage, so be sure to only add properties, keeping the current sequence.
        //   If a property is needed to be referenced that didn't exist in an older version, be sure to account
        //   for the possibility that the new property will have a default and meaningless value.
        //   EVEN SO, be sure to update the "ThisVersion" constant so that you can do a rudimentary version check to handle new properties
        //   that might not be set properly in an older version.
        [NetworkMessage(Sequence = 4)]
        public short AlertItem { get; private set; }
        [NetworkMessage(Sequence = 5)]
        public MessageString RelatedData { get; private set; }

        protected override void SetCommand()
        {
            Command = MessageCommand.AretmisAction;
            MessageVersion = ThisVersion;
        }
    }
}
