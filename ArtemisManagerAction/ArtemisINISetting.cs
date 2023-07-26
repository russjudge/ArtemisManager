using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisINISetting : INISetting, INotifyPropertyChanged
    {
        /*
        public ArtemisINISetting(string key, string value, bool useDefault = false) : base(key, value, useDefault) { }
        public ArtemisINISetting(string key, int value, bool useDefault = false) : base(key, value, useDefault) { }
        public ArtemisINISetting(string key, double value, bool useDefault = false) : base(key, value, useDefault) { }
        public ArtemisINISetting(string key, bool value, bool useDefault = false) : base(key, value, useDefault) { }

        public ArtemisINISetting(string key, Ship value, bool useDefault = false) : base(key, (int)value, useDefault) { }
        */
        
        public ArtemisINISetting() : base() { }

        private bool IsCommentLine(string line)
        {
            int posEqual = line.IndexOf('=');
            bool isComment = false;
            if (posEqual < 0)
            {
                isComment = true;
            }
            else
            {
                if (line.Substring(0, posEqual).Contains(' '))
                {
                    isComment = true;
                }
            }
            return isComment;
        }
        public override void ProcessLine(string line)
        {
            if (!string.IsNullOrEmpty(SettingName))
            {
                throw new InvalidOperationException("SettingName is already set--cannot process more lines.");
            }
            
            if (IsCommentLine(line))
            {
                CommentLines.Add(line);
            }
            else
            {
                bool useDefault = false;
                int startPos = 0;
                if (line[0] == ';')
                {
                    useDefault = true;
                    startPos++;
                }
                int posEqual = line.IndexOf('=');

                SettingName = line.Substring(startPos, posEqual - startPos);
                FileValue = line.Substring(posEqual + 1).Trim();
                UsingDefault = useDefault;
            }
        }
    }
}
