using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ArtemisINISetting : INotifyPropertyChanged
    {
        //Notes for artemis.ini:

        //a setting will have no space before or after the equal sign.

        //There are lines that appear to be comment lines that start "// replaced editable torpedo values".  These may be safe to drop.
        //Not all settings have comments before.

        //Valid types:
        // 0 or 1 for boolean
        // int
        //decimal/float/double
        //string (is always a file path.  a relative file path, relative to the folder artemis sbs is running from.
        //  It may be possible to fully qualify a path not part of artemis, but this would need tested.  True Mods MUST use the relative path.)

        //End of file marked:
        //; end of file

        //controls.ini follow different rules.  Equal sign has spaces before and after.
        internal ArtemisINISetting()
        {
        }
        internal ArtemisINISetting(string settingName, string initialValue = "", bool useDefault = false)
        {
            SettingName = settingName;
            FileValue = initialValue;
            UsingDefault = useDefault;
        }
        internal ArtemisINISetting(string settingName, int value = 0, bool useDefault = false)
        {
            SettingName = settingName;
            SetValue(value);
            UsingDefault = useDefault;
        }
        internal ArtemisINISetting(string settingName, double value = 0.0, bool useDefault = false)
        {
            SettingName = settingName;
            SetValue(value);
            UsingDefault = useDefault;
        }
        internal ArtemisINISetting(string settingName, bool value = false, bool useDefault = false)
        {
            SettingName = settingName;
            SetValue(value);
            UsingDefault = useDefault;
        }
        private static bool IsCommentLine(string line)
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
        public void ProcessLine(string line)
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
                    do
                    {
                        startPos++;
                    } while (line[startPos] == ' ');
                }
                int posEqual = line.IndexOf('=');

                SettingName = line.Substring(startPos, posEqual - startPos);
                FileValue = line.Substring(posEqual + 1).Trim();
                UsingDefault = useDefault;
            }
        }

        public List<string> CommentLines { get; set; } = [];
        private string settingName = string.Empty;
        public string SettingName
        {
            get
            {
                return settingName;
            }
            set
            {
                settingName = value;
                DoChanged();
            }
        }
        private string fileValue = string.Empty;
        public string FileValue
        {
            get
            {
                return fileValue;
            }
            set
            {
                fileValue = value;
                UsingDefault = false;
                DoChanged();
            }
        }
        public bool BoolValue
        {
            get
            {
                return GetBoolValue();
            }
            set
            {
                SetValue(value);
                DoChanged();
            }
        }
        public bool GetBoolValue()
        {
            return (FileValue == "1");
        }
        public double DoubleValue
        {
            get
            {
                return GetDoubleValue();
            }
            set
            {
                SetValue(value);
                DoChanged();
            }
        }
        public double GetDoubleValue()
        {
            if (double.TryParse(FileValue, out double retVal))
            {

            }
            return retVal;
        }
        public int IntValue
        {
            get
            {
                return GetIntValue();
            }
            set
            {
                SetValue(value);
                DoChanged();
            }
        }
        public int GetIntValue()
        {
            if (int.TryParse(FileValue, out int retVal))
            {

            }
            return retVal;
        }
        public void SetValue(string value)
        {
            FileValue = value;
        }
        public void SetValue(bool value)
        {
            FileValue = value ? "1" : "0";
        }
        public void SetValue(int value)
        {
            FileValue = value.ToString();
        }
        public void SetValue(double value)
        {
            FileValue = value.ToString();
            if (!FileValue.Contains('.'))
            {
                FileValue += ".0";
            }
            int i = FileValue.IndexOf('.');
            if (i < FileValue.Length - 2)
            {
                FileValue = string.Concat(FileValue.AsSpan(0, i), FileValue.AsSpan(i, 2));
            }
            //Decimal points found: 0.00, 0.0
        }
        private bool commentedOut = false;
        /// <summary>
        /// Sets whether or not the line is commented out.
        /// Note: When processing line, be sure to set this LAST (after FileValue),
        /// because FileValue sets UsingDefault to true when set.
        /// </summary>
        public bool UsingDefault
        {
            get
            {
                return commentedOut;
            }
            set
            {
                commentedOut = value;
                DoChanged();
            }
        }
        public string[] GetOutLines()
        {
            List<string> retVal = [.. CommentLines];
            StringBuilder sb = new();
            if (UsingDefault)
            {
                sb.Append(';');
            }
            sb.Append(SettingName);
            sb.Append('=');
            sb.Append(FileValue);
            retVal.Add(sb.ToString());
            return [.. retVal];
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
        public override string ToString()
        {
            return string.Join(Environment.NewLine, GetOutLines());
        }

    }
}
