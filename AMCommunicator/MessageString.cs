using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class MessageString
    {
        public MessageString() : this(BitConverter.GetBytes((int)0)) { }
        public MessageString(byte[] bytes) : this(bytes, 0) { }
        public MessageString(byte[] bytes, int position)
        {
            Length = BitConverter.ToInt32(bytes, position);
            if (Length > bytes.Length - (position + 4))
            {
                Length = bytes.Length - (position + 4);
            }
            if (Length > 4)
            {
                Message = System.Text.ASCIIEncoding.ASCII.GetString(bytes, position + 4, Length - 4);
            }
            else
            {
                Message = string.Empty;
            }
        }
        public MessageString(string message) 
        {
            Message = message;
            Length = message.Length + 4;
        }
        public int Length { get; private set; }
        public string Message { get; private set; }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Length));
            bytes.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes(Message));
            return bytes.ToArray();
        }
        public override string ToString()
        {
            return Message;
        }
    }
}
