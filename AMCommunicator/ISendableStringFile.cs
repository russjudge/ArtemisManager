using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public interface ISendableStringFile
    {
        Messages.SendableStringPackageFile FileType { get; }
        string GetSerializedString();
        string SaveFile { get; set; }
    }
}