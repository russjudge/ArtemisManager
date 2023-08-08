using ArtemisEngineeringPresets;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class EngineeringPresetFileListItem : FileListItem
    {
        public EngineeringPresetFileListItem(string name) : base(name) { }
        public PresetsFile? INIFile { get => base.SettingsFile as PresetsFile; set => base.SettingsFile = value; }
    }
}
