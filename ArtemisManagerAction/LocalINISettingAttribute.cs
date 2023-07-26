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
        public LocalINISettingAttribute(int sequence, ClientServerType _clientServerType) : base(sequence, _clientServerType) { }
    }
}
