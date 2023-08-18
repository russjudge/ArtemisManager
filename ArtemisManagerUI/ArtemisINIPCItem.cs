using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisManagerUI
{
    public class ArtemisINIPCItem : DependencyObject
    {
        //public ArtemisINIPCItem(PCItem pc, ArtemisINI settingsFile)
        //{
        //    Connection = pc;
        //    IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
        //    ArtemisSettingsFile = settingsFile;
        //}
        public ArtemisINIPCItem(PCItem pc)
        {
            Connection = pc;
            IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
            ArtemisSettingsFiles = new ObservableCollection<ArtemisINIFileListItem>();
        }
        
        public static readonly DependencyProperty IsRemoteProperty =
          DependencyProperty.Register(nameof(IsRemote), typeof(bool),
          typeof(ArtemisINIPCItem));


        public bool IsRemote
        {
            get
            {
                return (bool)this.GetValue(IsRemoteProperty);

            }
            set
            {
                this.SetValue(IsRemoteProperty, value);
            }
        }
        public static readonly DependencyProperty ConnectionProperty =
           DependencyProperty.Register(nameof(Connection), typeof(PCItem),
           typeof(ArtemisINIPCItem));


        public PCItem Connection
        {
            get
            {
                return (PCItem)this.GetValue(ConnectionProperty);

            }
            set
            {
                this.SetValue(ConnectionProperty, value);
            }
        }



        public static readonly DependencyProperty SelectedSettingsFileProperty =
           DependencyProperty.Register(nameof(SelectedSettingsFile), typeof(ArtemisINIFileListItem),
           typeof(ArtemisINIPCItem));

      

        public ArtemisINIFileListItem SelectedSettingsFile
        {
            get
            {
                return (ArtemisINIFileListItem)this.GetValue(SelectedSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedSettingsFileProperty, value);
            }
        }


        public static readonly DependencyProperty ArtemisSettingsFileProperty =
           DependencyProperty.Register(nameof(ArtemisSettingsFiles), typeof(ObservableCollection<ArtemisINIFileListItem>),
           typeof(ArtemisINIPCItem));


        public ObservableCollection<ArtemisINIFileListItem> ArtemisSettingsFiles
        {
            get
            {
                return (ObservableCollection<ArtemisINIFileListItem>)this.GetValue(ArtemisSettingsFileProperty);

            }
            set
            {
                this.SetValue(ArtemisSettingsFileProperty, value);
            }
        }
    }
}
