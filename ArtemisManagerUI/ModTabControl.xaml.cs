using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Interaction logic for ModTabControl.xaml
    /// </summary>
    public partial class ModTabControl : UserControl
    {
        public ModTabControl()
        {
            InitializeComponent();
            if (TakeAction.ConnectedPCs != null)
            {
                ConnectedPCs = TakeAction.ConnectedPCs;
            }
        }
        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
              typeof(ModTabControl));

        public ObservableCollection<PCItem> ConnectedPCs
        {
            get
            {
                return (ObservableCollection<PCItem>)this.GetValue(ConnectedPCsProperty);

            }
            set
            {
                this.SetValue(ConnectedPCsProperty, value);

            }
        }
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
            typeof(ModTabControl));

        public PCItem SelectedTargetPC
        {
            get
            {
                return (PCItem)this.GetValue(SelectedTargetPCProperty);

            }
            set
            {
                this.SetValue(SelectedTargetPCProperty, value);
            }
        }

        public static readonly DependencyProperty IsMasterProperty =
           DependencyProperty.Register(nameof(IsMaster), typeof(bool),
          typeof(ModTabControl));

        public bool IsMaster
        {
            get
            {
                return (bool)this.GetValue(IsMasterProperty);

            }
            set
            {
                this.SetValue(IsMasterProperty, value);
            }
        }
       

        public static readonly DependencyProperty ShowModsProperty =
           DependencyProperty.Register(nameof(ShowMods), typeof(bool),
          typeof(ModTabControl));

        public bool ShowMods
        {
            get
            {
                return (bool)this.GetValue(ShowModsProperty);

            }
            set
            {
                this.SetValue(ShowModsProperty, value);
            }
        }

       

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {

        }

        private void OnImport(object sender, RoutedEventArgs e)
        {
            ModInstallWindow win = new()
            {
                
                ForInstall = true
            };
            win.Mod.IsMission = !ShowMods;
            if (win.ShowDialog() == true)
            {
                if (win.Mod.IsMission)
                {
                    SelectedTargetPC.InstalledMissions.Add(win.Mod);
                    
                }
                else
                {
                    SelectedTargetPC.InstalledMods.Add(win.Mod);
                }
                TakeAction.SendClientInfo(TakeAction.AllConnections);
            }
        }

       
    }
}
