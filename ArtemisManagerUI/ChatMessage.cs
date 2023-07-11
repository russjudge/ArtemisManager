using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class ChatMessage
    {
        public ChatMessage(string source, string message)
        {
            Entry = DateTime.Now;
            Source = source;
            Message = message;
        }
        public string Source { get; private set; }
        public string Message { get; private set; }
        public DateTime Entry { get; private set; }

    }
}
