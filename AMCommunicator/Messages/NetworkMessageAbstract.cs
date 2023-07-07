using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator.Messages
{
    internal abstract class NetworkMessageAbstract : NetworkMessage
    {
        protected NetworkMessageAbstract() : base()
        {
            SetCommand();
        }
        protected NetworkMessageAbstract(byte[] bytes) : base(bytes)
        {
            SetCommand();
        }
        protected abstract void SetCommand();


    }
}
