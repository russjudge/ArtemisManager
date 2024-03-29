﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal class NetworkMessageHeader
    {
        public const int HeaderLength = 8;
        public const int JSONDefinitionLength = 4;
        public const int JSONDefinitionLengthPosition = 0;
        public NetworkMessageHeader(NetworkMessage message)
        {

            string typeParse = message.GetType().Name;
            typeParse = typeParse.Substring(0, typeParse.Length - 7);
            if (Enum.TryParse<MessageCommand>(typeParse, out MessageCommand cmd))
            {
                Command = cmd;
            }
            else
            {
                Command = MessageCommand.UndefinedPackage;
            }
            //Command = Enum.Parse <MessageCommand>(typeParse);

            //if (message.GetType() == typeof(AlertMessage))
            //{
            //    Command = MessageCommand.Alert;
            //}
            //else if (message.GetType() == typeof(ArtemisActionMessage))
            //{
            //    Command = MessageCommand.AretmisAction;
            //}
            //else if (message.GetType() == typeof(ChangeAppSettingMessage))
            //{
            //    Command = MessageCommand.ChangeAppSetting;
            //}
            //else if (message.GetType() == typeof(ChangePasswordMessage))
            //{
            //    Command = MessageCommand.ChangePassword;
            //}
            //else if (message.GetType() == typeof(ClientInfoMessage))
            //{
            //    Command= MessageCommand.ClientInfo;
            //}
            //else if (message.GetType() == typeof(CommunicationMessage))
            //{
            //    Command = MessageCommand.Communication;
            //}
            //else if (message.GetType() == typeof(HandshakeMessage))
            //{
            //    Command = MessageCommand.Handshake;
            //}
            //else if (message.GetType() == typeof(ModPackageMessage))
            //{
            //    Command = MessageCommand.ModPackage;
            //}
            //else if (message.GetType() == typeof(PCActionMessage))
            //{
            //    Command = MessageCommand.PCAction;
            //}
            //else if (message.GetType() == typeof(PingMessage))
            //{
            //    Command = MessageCommand.Ping;
            //}
            //else if (message.GetType() == typeof(RequestModPackageMessage))
            //{
            //    Command = MessageCommand.RequestModPackage;
            //}
            //else if (message.GetType() == typeof(StringPackageMessage))
            //{
            //    Command = MessageCommand.StringPackage;
            //}
            //else if (message.GetType() == typeof(RequestInformationMessage))
            //{
            //    Command = MessageCommand.RequestInformation;
            //}
            //else if (message.GetType() == typeof(InformationMessage))
            //{
            //    Command = MessageCommand.Information;
            //}
            //else
            //{
            //    Command = MessageCommand.UndefinedPackage;
            //}
            Version = message.MessageVersion;
            JSON = message.GetJSON();
            Length = JSON.Length;
        }
        public NetworkMessageHeader(byte[] data)
        {
            Length = BitConverter.ToInt32(data, 0);  //Is ONLY the length of the JSON part.
            Command = (MessageCommand)BitConverter.ToInt16(data, 4);
            Version = BitConverter.ToInt16(data, 6);

            JSON = System.Text.ASCIIEncoding.ASCII.GetString(data, 8, Length);
        }
        /// <summary>
        /// The length of the JSON string of the NetworkMessage object.
        /// </summary>
        public int Length { get; private set; }
        public MessageCommand Command { get; private set; }
        public short Version { get; private set; }

        public string JSON { get; private set; }
        public byte[] GetBytes()
        {
            List<byte> bytes =
            [
                .. BitConverter.GetBytes(Length),
                .. BitConverter.GetBytes((short)Command),
                .. BitConverter.GetBytes(Version),
                .. System.Text.ASCIIEncoding.ASCII.GetBytes(JSON),
            ];
            return [.. bytes];
        }
        public T? GetItem<T>()
        {
            return JsonSerializer.Deserialize<T>(JSON, NetworkMessage.jsonSerializerOptions);
        }
    }
}
