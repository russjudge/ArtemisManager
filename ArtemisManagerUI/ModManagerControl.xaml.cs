using AMCommunicator;
using AMCommunicator.Messages;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for ModManagerControl.xaml
    /// </summary>
    public partial class ModManagerControl : UserControl
    {
        public ModManagerControl()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
            typeof(ModManagerControl), new PropertyMetadata(OnSelectedTargetPCChanged));

        private static void OnSelectedTargetPCChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModManagerControl me)
            {
                me.IsRemote = !IPAddress.Loopback.Equals(me.SelectedTargetPC?.IP);
            }
        }

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
         typeof(ModManagerControl));

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
          typeof(ModManagerControl));

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
        public static readonly DependencyProperty IsRemoteProperty =
          DependencyProperty.Register(nameof(IsRemote), typeof(bool),
         typeof(ModManagerControl));

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
        private void OnModUninstalled(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is ModItem mod)
            {
                if (IsRemote)
                {
                    if (SelectedTargetPC.IP != null)
                    {
                        Network.Current?.SendArtemisAction(SelectedTargetPC.IP, AMCommunicator.Messages.ArtemisActions.UninstallMod, mod.ModIdentifier, mod.GetJSON());
                    }
                }
                else
                {
                    mod.Uninstall();
                    if (mod.IsMission)
                    {
                        SelectedTargetPC.InstalledMissions.Remove(mod);
                    }
                    else
                    {
                        SelectedTargetPC.InstalledMods.Remove(mod);
                    }
                    TakeAction.SendClientInfo(TakeAction.AllConnections);
                }
            }
        }

        private void OnDeactivateAllMods(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (SelectedTargetPC != null && SelectedTargetPC.IP != null)
                {
                    Network.Current?.SendArtemisAction(SelectedTargetPC.IP, ArtemisActions.ResetToVanilla, Guid.Empty, string.Empty);
                }
            }
            else
            {
                var baseItem = ArtemisManager.ClearActiveFolder();
                DeactivateAllButBase(baseItem?.LocalIdentifier);

                baseItem?.Activate();
                TakeAction.SendClientInfo(IPAddress.Any);
            }
        }
        private void DeactivateAllButBase(Guid? activeIdentifier)
        {
            foreach (var mod in SelectedTargetPC.InstalledMods)
            {
                if (mod.LocalIdentifier == activeIdentifier)
                {
                    mod.IsActive = true;
                }
                else
                {
                    mod.IsActive = false;
                }
            }
        }

        private void OnModActivated(object sender, RoutedEventArgs e)
        {

            if (e.OriginalSource is ModItem mod)
            {
                if (IsRemote)
                {
                    if (SelectedTargetPC.IP != null)
                    {
                        Network.Current?.SendArtemisAction(SelectedTargetPC.IP, AMCommunicator.Messages.ArtemisActions.ActivateMod, mod.ModIdentifier, mod.GetJSON());
                    }
                }
                else
                {
                    mod.Activate();
                    TakeAction.SendClientInfo(TakeAction.AllConnections);
                }
            }
        }
    }
}
