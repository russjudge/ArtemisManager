using AMCommunicator;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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

        public static readonly DependencyProperty IsMasterProperty =
           DependencyProperty.Register(nameof(IsMaster), typeof(bool),
          typeof(ModItemControl));

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
            RaiseModActivatedEvent();
        }

        private void OnUninstallMod(object sender, RoutedEventArgs e)
        {
            RaiseModUninstalledEvent();
        }

        void RaiseModActivatedEvent()
        {
            RoutedEventArgs args = new(ModActivatedEvent, Mod);
            RaiseEvent(args);
        }

        public static readonly RoutedEvent ModActivatedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ModActivated),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(ModItemControl));
        public event RoutedEventHandler ModActivated
        {
            add { AddHandler(ModActivatedEvent, value); }
            remove { RemoveHandler(ModActivatedEvent, value); }
        }

        void RaiseModUninstalledEvent()
        {
            RoutedEventArgs args = new(ModUninstalledEvent, Mod);
            RaiseEvent(args);
        }

        public static readonly RoutedEvent ModUninstalledEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ModUninstalled),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(ModItemControl));
        public event RoutedEventHandler ModUninstalled
        {
            add { AddHandler(ModUninstalledEvent, value); }
            remove { RemoveHandler(ModUninstalledEvent, value); }
        }
        bool isLoading = true;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (isLoading)
            {
                this.InitializeDragging("DragScope1");

                this.SetDragTypes(typeof(ModItemControl));
                isLoading = false;
            }
        }

    }
}
