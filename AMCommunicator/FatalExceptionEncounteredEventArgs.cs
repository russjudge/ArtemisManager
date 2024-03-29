﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class FatalExceptionEncounteredEventArgs : EventArgs
    {
        private FatalExceptionEncounteredEventArgs() => throw new NotImplementedException();
        public Exception Exception { get; private set; }
        public FatalExceptionEncounteredEventArgs(Exception exception)
        { this.Exception = exception; }
    }

}
