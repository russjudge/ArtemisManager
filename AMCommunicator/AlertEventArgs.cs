using AMCommunicator.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class AlertEventArgs : EventArgs
    {
        private AlertEventArgs() { AlertItem = AlertItems.Uninstall_Failure; RelatedData = string.Empty; }
        public AlertEventArgs(IPAddress? source, AlertItems alertItem, string relatedData)
        {
            AlertItem = alertItem;
            RelatedData = relatedData;
            Source = source;
        }
        public IPAddress? Source { get; private set; }
        public AlertItems AlertItem { get; private set; }
        public string RelatedData { get; private set; }
    }
}
