using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for TextEditorControl.xaml
    /// </summary>
    public partial class TextEditorControl : UserControl
    {
        public TextEditorControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(TextEditorControl));

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


        public static readonly DependencyProperty TargetClientProperty =
           DependencyProperty.Register(nameof(TargetClient), typeof(IPAddress),
           typeof(TextEditorControl));


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
        public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(TextDataFile),
            typeof(TextEditorControl));

        public TextDataFile Data
        {
            get
            {
                return (TextDataFile)this.GetValue(DataProperty);

            }
            set
            {
                this.SetValue(DataProperty, value);

            }
        }
        private void OnSave(object sender, RoutedEventArgs e)
        {
            if (TakeAction.IsLoopback(TargetClient))
            {
                using (StreamWriter sw = new StreamWriter(Data.SaveFile))
                {
                    sw.Write(Data.data);
                }
            }
            else
            {
                //TODO: Remotely save

            }
        }

        private void OnActivate(object sender, RoutedEventArgs e)
        {
            if (TakeAction.IsLoopback(TargetClient))
            {
                string target = string.Empty;
                switch (Data.FileType)
                {
                    case AMCommunicator.Messages.SendableStringPackageFile.DMXCommandsXML:
                        target = System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisDATSubfolder, ArtemisManager.DMXCommands);
                        break;
                    case AMCommunicator.Messages.SendableStringPackageFile.controlsINI:
                        target = System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.controlsINI);
                        break;
                }
                System.IO.File.Copy(Data.SaveFile, target, true);
            }
            else
            {
                //TODO: Remotely activate
            }
        }
    }
}
