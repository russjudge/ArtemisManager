using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ArtemisActionEventArgs : EventArgs
    {
        public ArtemisActionEventArgs(IPAddress? source, ArtemisActions action, string identifier, string? mod = null) 
        {
            Action = action;
            Identifier = Guid.Parse(identifier);
            Mod = mod;
            Source = source;
        }
        public IPAddress? Source { get;private set; }
        public ArtemisActions Action { get; private set; }
        public Guid Identifier { get; private set; }
        public string? Mod { get; private set; }
    }
}
