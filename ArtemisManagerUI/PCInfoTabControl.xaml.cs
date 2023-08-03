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
    /// Interaction logic for PCInfoTabControl.xaml
    /// </summary>
    public partial class PCInfoTabControl : UserControl
    {
        public PCInfoTabControl()
        {
            InitializeComponent();
            if (TakeAction.ConnectedPCs != null)
            {
                ConnectedPCs = TakeAction.ConnectedPCs;
            }
        }
        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
              typeof(PCInfoTabControl));

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
            typeof(PCInfoTabControl));

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
          typeof(PCInfoTabControl));

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
    }
}
