using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal class MessageString
    {
        public MessageString() : this(BitConverter.GetBytes(0)) { }
        public MessageString(byte[] bytes) : this(bytes, 0) { }
        public MessageString(byte[] bytes, int position)
        {
            //bytes.length = 26.  Length = 20.  Position = 6
            Length = BitConverter.ToInt32(bytes, position);

            if (Length > bytes.Length - position)
            {
                Length = bytes.Length - position;
            }
            position += 4;
            if (Length > 4)
            {
                Message = Encoding.ASCII.GetString(bytes, position, Length - 4);
            }
            else
            {
                Message = string.Empty;
            }
        }
        public MessageString(string message)
        {
            SetMessage(message);
        }
        public int Length { get; private set; }
        public string Message { get; private set; }
        public void SetMessage(string message)
        {
            Message = message;
            Length = message.Length + 4;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(Message));
            return bytes.ToArray();
        }
        public override string ToString()
        {
            return Message;
        }
    }
}
