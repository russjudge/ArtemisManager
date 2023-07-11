using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal abstract class NetworkMessage : INetworkMessage
    {
        protected NetworkMessage()
        {
            SetMessageVersion();
            Source = Network.GetMyIP();
            Unspecified = Array.Empty<byte>();
        }
        public short MessageVersion { get; set; }
        public IPAddress? Source { get; set; }
        public byte[] Unspecified { get; set; }

        protected abstract void SetMessageVersion();

        internal string GetJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(this, options);
        }
        
    }
}
