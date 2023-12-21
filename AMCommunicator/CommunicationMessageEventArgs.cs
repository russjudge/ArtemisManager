using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class CommunicationMessageEventArgs : EventArgs
    {
        private CommunicationMessageEventArgs() { Message = string.Empty; }
        public CommunicationMessageEventArgs(string? host, string message)
        {
            Host = host;
            Message = message;
        }
        public string Message { get; private set; }
        public string? Host { get; private set; }
    }
}
