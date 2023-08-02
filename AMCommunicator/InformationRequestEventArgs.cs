using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class InformationRequestEventArgs : EventArgs
    {
        public InformationRequestEventArgs(IPAddress? source, RequestInformationType requestType, string identifier) 
        {
            Source = source;
            RequestType = requestType;
            Identifier = identifier;
        }
        public IPAddress? Source { get; private set; }
        public RequestInformationType RequestType { get; private set; }
        public string Identifier { get;private set; }
    }
}
