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
    /// Interaction logic for VideoScreenSettingControl.xaml
    /// </summary>
    public partial class VideoScreenSettingControl : UserControl
    {
        public VideoScreenSettingControl()
        {
            AvailableModes = new ObservableCollection<KeyValuePair<VideoMode, string>>
            {
                new(VideoMode.None, VideoMode.None.ToString()),
                new (VideoMode.Windowed, VideoMode.Windowed.ToString()),
                new (VideoMode.FullScreen, "Full Screen"),
                new (VideoMode.FullScreenWindowed, "Full Screen Windowed")
            };
            AvailableResolutions = new();
            InitializeComponent();
        }
        public static readonly DependencyProperty SettingsFileProperty =
            DependencyProperty.Register(nameof(SettingsFile), typeof(ArtemisINI),
             typeof(VideoScreenSettingControl));

        public ArtemisINI SettingsFile
        {
            get
            {
                return (ArtemisINI)this.GetValue(SettingsFileProperty);

            }
            set
            {
                this.SetValue(SettingsFileProperty, value);

            }
        }

        public static readonly DependencyProperty AvailableResolutionsProperty =
           DependencyProperty.Register(nameof(AvailableResolutions), typeof(ObservableCollection<Size>),
            typeof(VideoScreenSettingControl));

        public ObservableCollection<Size> AvailableResolutions
        {
            get
            {
                return (ObservableCollection<Size>)this.GetValue(AvailableResolutionsProperty);

            }
            set
            {
                this.SetValue(AvailableResolutionsProperty, value);

            }
        }


        public static readonly DependencyProperty AvailableModesProperty =
           DependencyProperty.Register(nameof(AvailableModes), typeof(ObservableCollection<KeyValuePair<VideoMode, string>>),
            typeof(VideoScreenSettingControl));

        public ObservableCollection<KeyValuePair<VideoMode, string>> AvailableModes
        {
            get
            {
                return (ObservableCollection<KeyValuePair<VideoMode, string>>)this.GetValue(AvailableModesProperty);

            }
            set
            {
                this.SetValue(AvailableModesProperty, value);

            }
        }
    }
}
