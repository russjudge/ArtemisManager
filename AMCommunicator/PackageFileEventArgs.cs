using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class PackageFileEventArgs : EventArgs
    {
        internal PackageFileEventArgs(string serializedString, SendableStringPackageFile fileType, string filename)
        {
            Filename = filename;
            SerializedString = serializedString;
            FileType = fileType;
        }
        public string Filename { get; set; }
        public string SerializedString { get; set; }
        public SendableStringPackageFile FileType { get; set; }
    }
}
