using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class VersionMismatchEventArgs
    {
        public VersionMismatchEventArgs(short expectedVersion, short actualVersion, IPAddress source)
        {
            ExpectedVersion = expectedVersion;
            ActualVersion = actualVersion;
            Source = source;
        }
    
        public short ExpectedVersion { get; private set; }
        public short ActualVersion { get; private set; }
        public IPAddress Source { get; private set; }
    }
}
