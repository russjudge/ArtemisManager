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
    public class TextDataPCItem : DependencyObject
    {
        //public ArtemisINIPCItem(PCItem pc, ArtemisINI settingsFile)
        //{
        //    Connection = pc;
        //    IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
        //    ArtemisSettingsFile = settingsFile;
        //}
        public TextDataPCItem(PCItem pc)
        {
            Connection = pc;
            IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
            DataFileList = [];
        }

        public static readonly DependencyProperty IsRemoteProperty =
          DependencyProperty.Register(nameof(IsRemote), typeof(bool),
          typeof(TextDataPCItem));


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
           typeof(TextDataPCItem));


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
           DependencyProperty.Register(nameof(SelectedSettingsFile), typeof(TextDataFile),
           typeof(TextDataPCItem), new PropertyMetadata(OnSelectedSettingsFileChanged));

        private static void OnSelectedSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextDataPCItem me)
            {
                if (me.SelectedSettingsFile == null)
                {

                }
            }
        }

        public TextDataFile SelectedSettingsFile
        {
            get
            {
                return (TextDataFile)this.GetValue(SelectedSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedSettingsFileProperty, value);
            }
        }


        public static readonly DependencyProperty DataFileListProperty =
           DependencyProperty.Register(nameof(DataFileList), typeof(ObservableCollection<TextDataFile>),
           typeof(TextDataPCItem));


        public ObservableCollection<TextDataFile> DataFileList
        {
            get
            {
                return (ObservableCollection<TextDataFile>)this.GetValue(DataFileListProperty);

            }
            set
            {
                this.SetValue(DataFileListProperty, value);
            }
        }
    }
}
