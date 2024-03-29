﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ModPackageEventArgs : EventArgs
    {
        private ModPackageEventArgs() : base() { throw new NotImplementedException(); }
        public ModPackageEventArgs(IPAddress? source, byte[] data, string mod)
        {
            Source = source;
            Data = data;
            this.Mod = mod;
        }
        public IPAddress? Source { get; private set; }
        public byte[] Data { get; private set; } = [];
        public string Mod { get; private set; } = string.Empty;
    }
}
