using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ActionCommandEventArgs : EventArgs
    {
        public ActionCommandEventArgs(ActionCommands action, bool force, IPAddress? source) 
        {
            this.Action = action;
            this.Force = force;
            this.Source = source;
        }
        public IPAddress? Source { get; private set; }
        public ActionCommands Action { get; private set; }
        public bool Force { get;private set; }
    }
}
