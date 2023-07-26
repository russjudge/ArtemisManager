using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class INISetting
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
    }
}
