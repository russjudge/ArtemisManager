using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class NetworkDebugEventArgs : EventArgs
    {
        public NetworkDebugEventArgs(TcpUdp connectionType, TheDirection direction, NetworkMessage message) 
        {
            ConnectionType= connectionType;
            Direction= direction;
            Message = message;
        }
        public enum TcpUdp
        {
            UDP,
            TCP
        }
        public enum TheDirection
        {
            Connecting,
            Sending,
            Receiving,
        }
        public TcpUdp ConnectionType { get; private set; }
        public TheDirection Direction { get; private set; }
        public NetworkMessage Message { get; private set; }
    }
}
