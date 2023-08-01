using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(PCItem connection)
        {
            Connection = connection;
        }
        public PCItem Connection { get; private set; }
    }
}
