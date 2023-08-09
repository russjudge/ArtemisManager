using AMCommunicator.Messages;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for TextFileManagerControl.xaml
    /// </summary>
    public partial class TextFileManagerControl : UserControl
    {
        public TextFileManagerControl()
        {
            DataFileList = new ObservableCollection<TextDataFile>();
            InitializeComponent();
        }
        void LoadControlsINIFiles()
        {
            LoadFiles(ArtemisManager.GetControlsINIFileList());
        }
        void LoadDMXCommandsFiles()
        {
            LoadFiles(ArtemisManager.GetDMXCommandsFileList());
        }
        void LoadFiles(string[] files)
        {
            foreach (var file in files)
            {
                TextDataFile f = new(FileType, file);
                DataFileList.Add(f);
            }
        }
        void InitializeFileList()
        {
            DataFileList.Clear();
            switch(FileType)
            {
                case SendableStringPackageFile.controlsINI:
                    LoadControlsINIFiles();
                    break;
                case SendableStringPackageFile.DMXCommandsXML:
                    LoadDMXCommandsFiles();
                    break;
            }
        }

        public static readonly DependencyProperty FileTypeProperty =
        DependencyProperty.Register(nameof(FileType), typeof(SendableStringPackageFile),
            typeof(TextFileManagerControl), new PropertyMetadata(OnFileTypeChanged));

        private static void OnFileTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl me)
            {
                me.InitializeFileList();
            }
        }

        public SendableStringPackageFile FileType
        {
            get
            {
                return (SendableStringPackageFile)this.GetValue(FileTypeProperty);

            }
            set
            {
                this.SetValue(FileTypeProperty, value);

            }
        }


        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(TextFileManagerControl));

        public string PopupMessage
        {
            get
            {
                return (string)this.GetValue(PopupMessageProperty);

            }
            set
            {
                this.SetValue(PopupMessageProperty, value);

            }
        }
        public static readonly DependencyProperty IsRemoteProperty =
            DependencyProperty.Register(nameof(IsRemote), typeof(bool),
            typeof(TextFileManagerControl), new PropertyMetadata(OnIsRemoteChanged));

        private static void OnIsRemoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl control)
            {
                control.InitializeFileList();
            }
        }
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


        public static readonly DependencyProperty TargetClientProperty =
           DependencyProperty.Register(nameof(TargetClient), typeof(IPAddress),
           typeof(TextFileManagerControl));


        public IPAddress? TargetClient
        {
            get
            {
                return (IPAddress?)this.GetValue(TargetClientProperty);

            }
            set
            {
                this.SetValue(TargetClientProperty, value);

            }
        }

        public static readonly DependencyProperty DataFileListProperty =
            DependencyProperty.Register(nameof(DataFileList), typeof(ObservableCollection<TextDataFile>),
                typeof(TextFileManagerControl));

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
        public static readonly DependencyProperty SelectedDataFileProperty =
            DependencyProperty.Register(nameof(SelectedDataFile), typeof(TextDataFile),
             typeof(TextFileManagerControl), new PropertyMetadata(OnSelectedArtemisSettingsFileChanged));

        private static void OnSelectedArtemisSettingsFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextFileManagerControl me && me.SelectedDataFile != null)
            {
                //me.SelectedArtemisSettingsFile.INIFile = ArtemisManager.GetArtemisINI(me.SelectedArtemisSettingsFile.Name);
            }
        }

        public TextDataFile? SelectedDataFile
        {
            get
            {
                return (TextDataFile?)this.GetValue(SelectedDataFileProperty);

            }
            set
            {
                this.SetValue(SelectedDataFileProperty, value);

            }
        }

        private void OnAddSettingsFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {

        }

        private void OnImportFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnActivateSettingsFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnSettingsFileRename(object sender, RoutedEventArgs e)
        {

        }

        private void OnDeleteSettingsFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnExportSettingsFile(object sender, RoutedEventArgs e)
        {

        }

        private void OnSettingsFilenameChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
