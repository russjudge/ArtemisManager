using AMCommunicator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisManagerUI
{
    public class FileRequestRoutedEventArgs : RoutedEventArgs
    {
        public FileRequestRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent) { }
        public ISendableJsonFile? File { get; set; } = null;

    }
}
