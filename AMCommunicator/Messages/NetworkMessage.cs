using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{

    internal class NetworkMessage : INetworkMessage
    {

        public static NetworkMessage? GetMessage(byte[] data)
        {
            NetworkMessage? retVal = null;
            MessageCommand cmd = (MessageCommand)BitConverter.ToInt16(data, CommandPosition);

            foreach (var tp in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (tp.GetInterface(nameof(INetworkMessage)) != null)
                {
                    if (tp.GetCustomAttribute<NetworkMessageCommandAttribute>() != null)
                    {
                        if (tp.GetCustomAttribute<NetworkMessageCommandAttribute>()?.Command == cmd)
                        {
                            var constructionInfo = tp.GetConstructor(new Type[] { data.GetType() });
                            if (constructionInfo != null)
                            {
                                retVal = (NetworkMessage)constructionInfo.Invoke(new object[] { data });
                                break;
                            }
                        }
                    }
                }
            }
            return retVal;
        }

        protected NetworkMessage()
        {
            Source = Network.GetMyIP();
            LoadPropertyDictionary();
            Unspecified = Array.Empty<byte>();

        }

        public NetworkMessage(byte[] bytes)
        {
            Source = IPAddress.None;
            Unspecified = Array.Empty<byte>();
            LoadPropertyDictionary();
            LoadBytes(bytes);
        }
        void LoadPropertyDictionary()
        {
            foreach (var property in GetType().GetProperties())
            {
                var attribute = property.GetCustomAttribute<NetworkMessageAttribute>();
                if (attribute != null)
                {
                    int seq = attribute.Sequence;
                    propertyDictionary.Add(seq, property);
                    sortedPropertyKeys.Add(seq);
                }
            }
            sortedPropertyKeys.Sort();
        }

        private void LoadBytes(byte[] bytes)
        {

            int position = 0;
            foreach (var key in sortedPropertyKeys)
            {
                if (position < bytes.Length)
                {
                    if (propertyDictionary[key].PropertyType == typeof(bool))
                    {
                        propertyDictionary[key].SetValue(this, BitConverter.ToBoolean(bytes, position));
                        position++;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(byte))
                    {
                        propertyDictionary[key].SetValue(this, bytes[position]);
                        position++;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(short))
                    {
                        propertyDictionary[key].SetValue(this, BitConverter.ToInt16(bytes, position));
                        position += 2;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(int))
                    {
                        propertyDictionary[key].SetValue(this, BitConverter.ToInt32(bytes, position));
                        position += 4;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(long))
                    {
                        propertyDictionary[key].SetValue(this, BitConverter.ToInt64(bytes, position));
                        position += 8;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(MessageString))
                    {
                        MessageString msg = new MessageString(bytes, position);
                        propertyDictionary[key].SetValue(this, msg);
                        position += msg.Length + 4;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(MessageCommand))
                    {
                        propertyDictionary[key].SetValue(this, (MessageCommand)BitConverter.ToInt16(bytes, position));
                        position += 2;
                    }
                    else if (propertyDictionary[key].PropertyType == typeof(IPAddress))
                    {
                        propertyDictionary[key].SetValue(this, new IPAddress(BitConverter.ToInt64(bytes, position)));
                        position += 8;

                    }

                }
                else
                {
                    break;
                }
            }
            if (position < bytes.Length - 1)
            {
                List<byte> list = new();
                for (int i = position; i < bytes.Length; i++)
                {
                    list.Add(bytes[i]);
                }
                Unspecified = list.ToArray();
            }
            else
            {
                Unspecified = Array.Empty<byte>();
            }
        }
        private readonly Dictionary<int, PropertyInfo> propertyDictionary = new();
        private readonly List<int> sortedPropertyKeys = new();

        public const int MinimumMessageLength = 4;
        
        public const int LengthSequence = 1;
        public const int LengthLength = 4;
        public const int LengthPosition = MessageVersionPosition + MessageVersionLength;
        public const int CommandSequence = 2;
        public const int MessageVersionPosition = 0;
        public const int MessageVersionSequence = 0;
        public const int MessageVersionLength = 2;
        public const int CommandPosition = LengthPosition + LengthLength;
        public const int MaxSequence = 99999;

        [NetworkMessage(Sequence = MessageVersionSequence)]
        public short MessageVersion { get; set; }

        [NetworkMessage(Sequence = LengthSequence)]
        public int Length { get; set; }

        [NetworkMessage(Sequence = CommandSequence)]
        public MessageCommand Command { get; set; }
        public IPAddress? Source { get; set; }


        [NetworkMessage(Sequence = MaxSequence)]
        public byte[] Unspecified { get; set; }


        public byte[] GetBytes()
        {
            List<byte> retVal = new();
            foreach (var key in sortedPropertyKeys)
            {
                var value = propertyDictionary[key].GetValue(this);

                if (propertyDictionary[key].PropertyType == typeof(bool))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes((bool)value));
                    }
                    else
                    {
                        retVal.Add(0);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(byte))
                {
                    if (value != null)
                    {
                        retVal.Add((byte)value);
                    }
                    else
                    {
                        retVal.Add(0);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(short))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes((short)value));
                    }
                    else
                    {
                        retVal.AddRange(new byte[2]);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(int))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes((int)value));
                    }
                    else
                    {
                        retVal.AddRange(new byte[4]);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(long))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes((long)value));
                    }
                    else
                    {
                        retVal.AddRange(new byte[8]);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(MessageString))
                {
                    if (value != null)
                    {
                        MessageString msg = (MessageString)value;
                        retVal.AddRange(msg.GetBytes());
                    }
                    else
                    {
                        retVal.AddRange(BitConverter.GetBytes(4));
                    }

                }
                else if (propertyDictionary[key].PropertyType == typeof(MessageCommand))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes((short)value));
                    }
                    else
                    {
                        retVal.AddRange(new byte[2]);
                    }
                }
                else if (propertyDictionary[key].PropertyType == typeof(IPAddress))
                {
                    if (value != null)
                    {
                        retVal.AddRange(BitConverter.GetBytes(((IPAddress)value).Address));
                    }
                    else
                    {
                        retVal.AddRange(new byte[8]);
                    }
                }

            }
            Length = retVal.Count;
            byte[] buffer = retVal.ToArray();
            var lengthBytes = BitConverter.GetBytes(Length);
            Array.Copy(lengthBytes, 0, buffer, LengthPosition, lengthBytes.Length);

            return buffer;
        }
    }
}
