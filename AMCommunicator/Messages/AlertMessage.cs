using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [NetworkMessageCommand(MessageCommand.Alert)]
    internal class AlertMessage : NetworkMessage
    {
        public const short ThisVersion = 0;  //Increment by 1 for each new release of the application that changes THIS NetworkMessage.

        public AlertMessage(AlertItems alert, string relatedData) : base()
        {
            AlertItem = alert;
            RelatedData = relatedData;
        }
        protected override void SetMessageVersion()
        {
            MessageVersion = ThisVersion;
        }
        
        //WARNING!!! There is no version check on this NetworkMessage, so be sure to only add properties, keeping the current sequence.
        //   If a property is needed to be referenced that didn't exist in an older version, be sure to account
        //   for the possibility that the new property will have a default and meaningless value.
        //   EVEN SO, be sure to update the "ThisVersion" constant so that you can do a rudimentary version check to handle new properties
        //   that might not be set properly in an older version.
        public AlertItems AlertItem { get; protected set; }
        public string RelatedData { get; protected set; }

      
    }
}
