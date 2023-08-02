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
    
    [JsonDerivedType(typeof(AlertMessage), nameof(AlertMessage))]
    [JsonDerivedType(typeof(ArtemisActionMessage), nameof(ArtemisActionMessage))]
    [JsonDerivedType(typeof(ChangeAppSettingMessage), nameof(ChangeAppSettingMessage))]
    [JsonDerivedType(typeof(ChangePasswordMessage), nameof(ChangePasswordMessage))]
    [JsonDerivedType(typeof(ClientInfoMessage), nameof(ClientInfoMessage))]
    [JsonDerivedType(typeof(CommunicationMessage), nameof(CommunicationMessage))]
    [JsonDerivedType(typeof(HandshakeMessage), nameof(HandshakeMessage))]
    [JsonDerivedType(typeof(ModPackageMessage), nameof(ModPackageMessage))]
    [JsonDerivedType(typeof(PCActionMessage), nameof(PCActionMessage))]
    [JsonDerivedType(typeof(PingMessage), nameof(PingMessage))]
    [JsonDerivedType(typeof(RequestModPackageMessage), nameof(RequestModPackageMessage))]
    [JsonDerivedType(typeof(StringPackageMessage), nameof(StringPackageMessage))]
    [JsonDerivedType(typeof(RequestInformationMessage), nameof(RequestInformationMessage))]
    [JsonDerivedType(typeof(InformationMessage), nameof(InformationMessage))]
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
                WriteIndented = false,
            };
            return JsonSerializer.Serialize<NetworkMessage>(this, options);
        }
    }
}
