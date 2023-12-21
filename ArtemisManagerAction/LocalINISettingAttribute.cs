using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class LocalINISettingAttribute : INISettingAttribute
    {
        private LocalINISettingAttribute() : base(0, ClientServerType.Both) { throw new NotImplementedException(); }
        public LocalINISettingAttribute(int sequence, ClientServerType _clientServerType) : base(sequence, _clientServerType) { }
    }
}
