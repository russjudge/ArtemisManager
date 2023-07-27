using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public interface ISendableJsonFile
    {
        Messages.JsonPackageFile FileType { get; }
        string GetJSON();
        string SaveFile { get; set; }
    }
}