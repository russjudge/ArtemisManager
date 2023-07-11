using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ItemRequestEventArgs : EventArgs
    {
        public ItemRequestEventArgs(IPAddress? target, string itemIdentifier)
        {

            Target = target;
            ItemIdentifier = itemIdentifier;
        }
        public IPAddress? Target { get; private set; }
        public string ItemIdentifier { get; private set; }
    }
}
