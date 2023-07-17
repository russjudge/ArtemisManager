using AMCommunicator;
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
    /// Interaction logic for ModItemControl.xaml
    /// </summary>
    public partial class ModItemControl : UserControl
    {
        public ModItemControl()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty SourceProperty =
         DependencyProperty.Register(nameof(Source), typeof(IPAddress),
             typeof(ModItemControl));

        public IPAddress? Source
        {
            get
            {
                return (IPAddress?)this.GetValue(SourceProperty);

            }
            set
            {
                this.SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty IsRemoteProperty =
         DependencyProperty.Register(nameof(IsRemote), typeof(bool),
             typeof(ModItemControl));

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

        public static readonly DependencyProperty ModProperty =
         DependencyProperty.Register(nameof(Mod), typeof(ModItem),
             typeof(ModItemControl));

        public ModItem Mod
        {
            get
            {
                return (ModItem)this.GetValue(ModProperty);

            }
            set
            {
                this.SetValue(ModProperty, value);
            }
        }

        private void OnActivateMod(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (Source != null)
                {
                    Network.Current?.SendArtemisAction(Source, AMCommunicator.Messages.ArtemisActions.ActivateMod, Mod.ModIdentifier, Mod.GetJSON());
                }
            }
            else
            {
                Mod.Activate();
            }
        }

        private void OnInstallMod(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (Source != null)
                {
                    Network.Current?.SendModPackageRequest(Source, Mod.GetJSON(), Mod.PackageFile);
                }
            }
            else
            {
               
            }
        }

        private void OnRemoteInstallMod(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (Source != null)
                {
                    Network.Current?.SendArtemisAction(Source, AMCommunicator.Messages.ArtemisActions.InstallMod, Mod.ModIdentifier, Mod.GetJSON());
                }
            }
           
        }
    }
}
