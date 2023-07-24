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
        internal PackageFileEventArgs(string json, JsonPackageFile fileType, string filename)
        {
            Filename = filename;
            JSON = json;
            FileType = fileType;
        }
        public string Filename { get; set; }
        public string JSON { get; set; }
        public JsonPackageFile FileType { get; set; }
    }
}
