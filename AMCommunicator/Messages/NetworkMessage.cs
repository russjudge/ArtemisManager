using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    [JsonDerivedType(typeof(HandshakeMessage))]
    [JsonDerivedType(typeof(AlertMessage))]
    [JsonDerivedType(typeof(ArtemisActionMessage))]
    [JsonDerivedType(typeof(ChangeAppSettingMessage))]
    [JsonDerivedType(typeof(ClientInfoMessage))]
    [JsonDerivedType(typeof(CommunicationMessage))]
    [JsonDerivedType(typeof(ItemMessage))]
    [JsonDerivedType(typeof(PCActionMessage))]
    [JsonDerivedType(typeof(PingMessage))]
    [JsonDerivedType(typeof(RequestItemMessage))]
    public abstract class NetworkMessage : INetworkMessage
    {
        protected NetworkMessage()
        {
            SetMessageVersion();
            Source = Network.GetMyIP()?.ToString();
            Unspecified = Array.Empty<byte>();
        }
        public short MessageVersion { get; set; }
        public string? Source { get; set; }
        public byte[] Unspecified { get; set; }

        protected abstract void SetMessageVersion();

        public string GetJSON()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            return JsonSerializer.Serialize<NetworkMessage>(this, options);
        }
    }
}
