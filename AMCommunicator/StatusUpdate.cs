using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class StatusUpdate: EventArgs
    {
        public StatusUpdate(string message, params object[] options)
        {
            Message = string.Format(message, options); 
        }
        public string Message { get; private set; }
    }
}
