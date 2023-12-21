using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class StatusUpdateEventArgs : EventArgs
    {
        private StatusUpdateEventArgs() { throw new NotImplementedException(); }
        public StatusUpdateEventArgs(string message, params object[] options)
        {
            Message = string.Format(message, options);
        }
        public string Message { get; private set; }
    }
}
