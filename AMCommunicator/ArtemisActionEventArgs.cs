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
        private ArtemisActionEventArgs() { Action = ArtemisActions.ActivateArtemisINIFile; Identifier = Guid.Empty; }
        public ArtemisActionEventArgs(IPAddress? source, ArtemisActions action, string identifier, string? mod = null, string? saveName = null)
        {
            Action = action;
            Identifier = Guid.Parse(identifier);
            Mod = mod;
            Source = source;
            SaveName = saveName;
        }
        public IPAddress? Source { get; private set; }
        public ArtemisActions Action { get; private set; }
        public Guid Identifier { get; private set; }
        public string? Mod { get; private set; }
        public string? SaveName { get; private set; }
    }
}
