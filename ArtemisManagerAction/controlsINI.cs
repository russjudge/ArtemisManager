using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
#pragma warning disable IDE1006 // Naming Styles
    public class controlsINI : INIFile
#pragma warning restore IDE1006 // Naming Styles
    {
        public controlsINI(string file) : base()
        {
            //LoadFile<controlsINI>(file);
        }

        public override SendableStringPackageFile FileType
        {
            get
            {
                return SendableStringPackageFile.controlsINI;
            }
        }
    }
}
