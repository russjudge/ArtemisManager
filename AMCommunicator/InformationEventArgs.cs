using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class InformationEventArgs : InformationRequestEventArgs
    {
        public InformationEventArgs(IPAddress? source, RequestInformationType requestType, string identifier, string[] data) : base(source, requestType, identifier)
        {
            
            Data = data;
        }
        
        public string[] Data { get; private set; }
        public bool Handled { get; set; } = false;
        
    }
}
