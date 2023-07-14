using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ModPackageRequestEventArgs : EventArgs
    {
        public ModPackageRequestEventArgs(IPAddress? target,string itemRequestedIdentifier, string modItem)
        {

            Target = target;
            ItemRequestedIdentifier = itemRequestedIdentifier;
            ModItem = modItem;
            
        }
        public IPAddress? Target { get; private set; }
        public string ItemRequestedIdentifier { get; private set; }
        public string ModItem { get; private set;}
    }
}
