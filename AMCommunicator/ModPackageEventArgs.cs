using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ModPackageEventArgs : EventArgs
    {
        public ModPackageEventArgs(byte[] data, string mod)
        {
            Data = data;
            this.Mod = mod;
        }

        public byte[] Data { get; private set; } = Array.Empty<byte>();
        public string Mod { get; private set; } = string.Empty;
    }
}
