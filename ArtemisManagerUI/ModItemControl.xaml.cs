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

        private void OnUninstallMod(object sender, RoutedEventArgs e)
        {
            if (IsRemote)
            {
                if (Source != null)
                {
                    Network.Current?.SendArtemisAction(Source, AMCommunicator.Messages.ArtemisActions.UninstallMod, Mod.ModIdentifier, Mod.GetJSON());
                }
            }
            else
            {
                Mod.Uninstall();
            }
        }
        Window? _dragdropWindow = null;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                data.SetData(this.GetType());
                data.SetData("Object", this);
                /*
                this.Effect = new DropShadowEffect
                {
                    Color = new Color { A = 50, R = 0, G = 0, B = 0 },
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = .75,
                };
                */
                _dragdropWindow = new Window();
                _dragdropWindow.WindowStyle = WindowStyle.None;
                _dragdropWindow.AllowsTransparency = true;
                _dragdropWindow.AllowDrop = false;
                _dragdropWindow.Background = null;
                _dragdropWindow.IsHitTestVisible = false;
                _dragdropWindow.SizeToContent = SizeToContent.WidthAndHeight;
                _dragdropWindow.Topmost = true;
                _dragdropWindow.ShowInTaskbar = false;

                Rectangle r = new Rectangle();
                r.Width = ((FrameworkElement)this).ActualWidth;
                r.Height = ((FrameworkElement)this).ActualHeight;
                r.Fill = new VisualBrush(this);
                this._dragdropWindow.Content = r;

                /*
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                

                this._dragdropWindow.Left = w32Mouse.X;
                this._dragdropWindow.Top = w32Mouse.Y;
                */
                var mousePosition = Mouse.GetPosition(Application.Current.MainWindow);
                this._dragdropWindow.Left = mousePosition.X;
                this._dragdropWindow.Top = mousePosition.Y;
                this._dragdropWindow.Show();

                // Initiate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy);
            }
        }
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            // These Effects values are set in the drop target's
            // DragOver event handler.
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Hand);
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }
    }
}
