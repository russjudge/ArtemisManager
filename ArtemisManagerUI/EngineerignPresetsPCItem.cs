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
    public class EngineeringPresetsPCItem : DependencyObject
    {
        //public ArtemisINIPCItem(PCItem pc, ArtemisINI settingsFile)
        //{
        //    Connection = pc;
        //    IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
        //    ArtemisSettingsFile = settingsFile;
        //}
        public EngineeringPresetsPCItem(PCItem pc)
        {
            Connection = pc;
            IsRemote = (pc.IP?.ToString() == IPAddress.Loopback.ToString());
            EngineeringPresetsFiles = new ObservableCollection<EngineeringPresetFileListItem>();
        }
        
        public static readonly DependencyProperty IsRemoteProperty =
          DependencyProperty.Register(nameof(IsRemote), typeof(bool),
          typeof(EngineeringPresetsPCItem));


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
           typeof(EngineeringPresetsPCItem));


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
           DependencyProperty.Register(nameof(SelectedSettingsFile), typeof(EngineeringPresetFileListItem),
           typeof(EngineeringPresetsPCItem), new PropertyMetadata(OnSelectedSettingsFileChanged));

        private static void OnSelectedSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetsPCItem me)
            {
                if (me.SelectedSettingsFile.INIFile == null)
                {

                }
            }
        }

        public EngineeringPresetFileListItem SelectedSettingsFile
        {
            get
            {
                return (EngineeringPresetFileListItem)this.GetValue(SelectedSettingsFileProperty);

            }
            set
            {
                this.SetValue(SelectedSettingsFileProperty, value);
            }
        }


        public static readonly DependencyProperty EngineeringPresetsFilesProperty =
           DependencyProperty.Register(nameof(EngineeringPresetsFiles), typeof(ObservableCollection<EngineeringPresetFileListItem>),
           typeof(EngineeringPresetsPCItem));


        public ObservableCollection<EngineeringPresetFileListItem> EngineeringPresetsFiles
        {
            get
            {
                return (ObservableCollection<EngineeringPresetFileListItem>)this.GetValue(EngineeringPresetsFilesProperty);

            }
            set
            {
                this.SetValue(EngineeringPresetsFilesProperty, value);
            }
        }
    }
}
