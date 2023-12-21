using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class INISettingAttribute : Attribute
    {
        private INISettingAttribute() { throw new NotImplementedException(); }
        public INISettingAttribute(int sequence, ClientServerType _clientServerType)
        {
            Sequence = sequence;
            clientServerType = _clientServerType;
        }
        public int Sequence { get; set; }
        public ClientServerType clientServerType { get; set; }
    }
}
