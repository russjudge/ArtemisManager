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

namespace ArtemisEngineeringPresets
{
    /// <summary>
    /// Interaction logic for PresetSettingsControl.xaml
    /// </summary>
    public partial class PresetSettingsControl : UserControl
    {
        public PresetSettingsControl()
        {
           
            InitializeComponent();
        }

        //static void OnPresetItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        //{
        //    PresetSettingsControl me = sender as PresetSettingsControl;
        //    if (me != null)
        //    {
        //        me.tb.SelectedIndex = -1;
        //        me.Dispatcher.BeginInvoke(new Action(me.SelectFirstItem), System.Windows.Threading.DispatcherPriority.Loaded);
        //    }
        //}
        //void SelectFirstItem()
        //{
        //    tb.SelectedIndex = 0;
        //}
        //public static readonly DependencyProperty PresetItemsProperty =
        //   DependencyProperty.Register("PresetItems", typeof(Preset[]),
        //   typeof(PresetSettingsControl));

        //public Preset[] PresetItems
        //{
        //    get
        //    {
        //        return (Preset[])this.GetValue(PresetItemsProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(PresetItemsProperty, value);
        //    }
        //}
        public static readonly DependencyProperty FileProperty =
           DependencyProperty.Register(nameof(File), typeof(PresetsFile),
           typeof(PresetSettingsControl));

        public PresetsFile File
        {
            get
            {
                return (PresetsFile)this.GetValue(FileProperty);
            }
            set
            {
                this.SetValue(FileProperty, value);
            }
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            File.Save();
        }
    }
}