using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class controlsINI : INIFile
    {
        public controlsINI(string file) : base()
        {
            LoadFile<controlsINI>(file);
        }


    }
}
