using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class DragStartedEventArgs : EventArgs
    {
        private DragStartedEventArgs() { throw new NotImplementedException(); }
        public DragStartedEventArgs(object dragObject)
        {
            DragObject = dragObject;
        }
        public object DragObject { get; private set; }
    }
}