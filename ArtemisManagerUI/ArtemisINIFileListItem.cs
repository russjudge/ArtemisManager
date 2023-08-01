﻿using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class ArtemisINIFileListItem : FileListItem
    {
        public ArtemisINIFileListItem(string name) : base(name) { }
        public  ArtemisINI? INIFile { get => base.SettingsFile as ArtemisINI; set => base.SettingsFile = value; }
    }
}
