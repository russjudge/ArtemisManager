using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class ChangeSettingEventArgs : EventArgs
    {
        private ChangeSettingEventArgs() { SettingName = string.Empty; SettingValue = string.Empty; }
        public ChangeSettingEventArgs(string settingName, string settingValue)
        {
            this.SettingName = settingName;
            this.SettingValue = settingValue;
        }
        public string SettingName { get; private set; }
        public string SettingValue { get; private set; }
    }
}
