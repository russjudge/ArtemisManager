using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
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
    /// Interaction logic for SpecialFileControl.xaml
    /// </summary>
    public partial class SpecialFileControl : UserControl
    {
        public const string ImageFilter = "Image Files|*.jpg;*.jpeg;*.png;*.jfif;*.bmp;*.gif;*.tif;*.tiff;*.ico|" + AllFilter;
        public const string AudioFilter = "Audio Files |*.mp3;*.wav;*.ogg;*.flac;*.aac;*.wma;*.m4a" + AllFilter;
        public const string AllFilter = "All Files (*.*)|*.*";
        public SpecialFileControl()
        {
            FileFilter = AllFilter;
            InitializeComponent();
        }
        public static readonly DependencyProperty FileFilterProperty =
        DependencyProperty.Register(nameof(FileFilter), typeof(string),
        typeof(SpecialFileControl));
        public string FileFilter
        {
            get
            {
                return (string)GetValue(FileFilterProperty);
            }
            set
            {
                this.SetValue(FileFilterProperty, value);
            }
        }

        public static readonly DependencyProperty IsImageProperty =
         DependencyProperty.Register(nameof(IsImage), typeof(bool),
         typeof(SpecialFileControl), new PropertyMetadata(OnIsImageChanged));

        private static void OnIsImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (me.IsImage)
                {
                    me.FileFilter = ImageFilter;
                }
            }
        }

        public bool IsImage
        {
            get
            {
                return (bool)GetValue(IsImageProperty);
            }
            set
            {
                this.SetValue(IsImageProperty, value);
            }
        }
        public static readonly DependencyProperty IsSoundProperty =
        DependencyProperty.Register(nameof(IsSound), typeof(bool),
        typeof(SpecialFileControl), new PropertyMetadata(OnIsSoundChanged));

        private static void OnIsSoundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (me.IsSound)
                {
                    me.FileFilter = AudioFilter;
                }
            }
        }

        public bool IsSound
        {
            get
            {
                return (bool)GetValue(IsSoundProperty);
            }
            set
            {
                this.SetValue(IsSoundProperty, value);
            }
        }

        public static readonly DependencyProperty UsingDefaultProperty =
           DependencyProperty.Register(nameof(UsingDefault), typeof(bool),
           typeof(SpecialFileControl));
        public bool UsingDefault
        {
            get
            {
                return (bool)GetValue(UsingDefaultProperty);
            }
            set
            {
                this.SetValue(UsingDefaultProperty, value);
            }
        }


        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register(nameof(IsPlaying), typeof(bool),
            typeof(SpecialFileControl));
        public bool IsPlaying
        {
            get
            {
                return (bool)GetValue(IsPlayingProperty);
            }
            set
            {
                this.SetValue(IsPlayingProperty, value);
            }
        }

        public static readonly DependencyProperty ImageProperty =
          DependencyProperty.Register(nameof(Image), typeof(ImageSource),
          typeof(SpecialFileControl));
        public ImageSource Image
        {
            get
            {
                return (ImageSource)GetValue(ImageProperty);
            }
            set
            {
                this.SetValue(ImageProperty, value);
            }
        }
        public static readonly DependencyProperty RelativeFolderPathProperty =
          DependencyProperty.Register(nameof(RelativeFolderPath), typeof(string),
          typeof(SpecialFileControl), new PropertyMetadata(OnRelativePathChanged));

        private static void OnRelativePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                //me.Filename = me.FileFolderPrefix + me.RelativeFolderPath;
                if (!me.IsSetting)
                {
                    me.IsSetting = true;
                    // Also, need to process for "/" 
                    // Also, validate RelativeFolderPath.  ONLY starting with "art" or "dat" is valid.
                    //  
                    while (me.RelativeFolderPath.StartsWith("\\"))
                    {
                        me.RelativeFolderPath = me.RelativeFolderPath.Substring(1);
                    }
                    while(me.RelativeFolderPath.EndsWith("\\"))
                    {
                        me.RelativeFolderPath = me.RelativeFolderPath.Substring(0, me.RelativeFolderPath.Length - 1);
                    }
                    me.SetFilename();
                    me.IsSetting = false;
                }
            }
        }

        public string RelativeFolderPath
        {
            get
            {
                return (string)GetValue(RelativeFolderPathProperty);
            }
            set
            {
                this.SetValue(RelativeFolderPathProperty, value);
            }
        }

        public static readonly DependencyProperty FileFolderPrefixProperty =
          DependencyProperty.Register(nameof(FileFolderPrefix), typeof(string),
          typeof(SpecialFileControl), new PropertyMetadata(OnFileFolderPrefixChanged));

        private static void OnFileFolderPrefixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (!me.IsSetting)
                {
                    me.IsSetting = true;
                    while (!string.IsNullOrEmpty(me.FileFolderPrefix) && me.FileFolderPrefix.EndsWith("\\"))
                    {
                        me.FileFolderPrefix = me.FileFolderPrefix.Substring(0, me.FileFolderPrefix.Length - 1);
                    }
                    if (!string.IsNullOrEmpty(me.RelativeFolderPath) && me.RelativeFolderPath.Contains(":"))
                    {
                        me.RelativeFolderPath = me.RelativeFolderPath.Replace(me.FileFolderPrefix, string.Empty);
                    }
                    me.SetFilename();
                    me.IsSetting = false;
                }
            }
        }
        void SetFilename()
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(FileFolderPrefix))
            {
                result = FileFolderPrefix;
            }
            string relativePath = RelativeFolderPath;
            string file = File;

            if (!string.IsNullOrEmpty(relativePath))
            {
                result = System.IO.Path.Combine(result, relativePath);
            }
            if (!string.IsNullOrEmpty(file))
            {
                result = System.IO.Path.Combine(result, file);
            }
            Filename = result;
        }
        public string FileFolderPrefix
        {
            get
            {
                return (string)GetValue(FileFolderPrefixProperty);
            }
            set
            {
                this.SetValue(FileFolderPrefixProperty, value);
            }
        }
        public static readonly DependencyProperty FileProperty =
          DependencyProperty.Register(nameof(File), typeof(string),
          typeof(SpecialFileControl), new PropertyMetadata(OnFileChanged));

        private static void OnFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (!me.IsSetting)
                {
                    me.IsSetting = true;
                    me.SetFilename();
                    me.IsSetting = false;
                }
            }
        }

        public string File
        {
            get
            {
                return (string)GetValue(FileProperty);
            }
            set
            {
                this.SetValue(FileProperty, value);
            }
        }
        bool IsSetting = false;

        public static readonly DependencyProperty ValueProperty =
         DependencyProperty.Register(nameof(Value), typeof(string),
         typeof(SpecialFileControl), new PropertyMetadata(OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (!me.IsSetting)
                {

                    me.IsSetting = true;
                    if (!string.IsNullOrEmpty(me.Value))
                    {
                        int i = me.Value.LastIndexOf(@"\");
                        if (i < 0)
                        {
                            i = me.Value.LastIndexOf("/");
                        }
                        if (i >= 0)
                        {
                            me.RelativeFolderPath = me.Value.Substring(0, i);
                            me.File = me.Value.Substring(i + 1, me.Value.Length - (i + 1));
                        }
                        else
                        {
                            me.RelativeFolderPath = string.Empty;
                            me.File = me.Value;
                        }
                    }
                    else
                    {
                        me.RelativeFolderPath = string.Empty;
                        me.File = string.Empty;
                    }
                    me.SetFilename();
                    me.IsSetting = false;
                }
            }
        }
        
        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }
            set
            {
                this.SetValue(ValueProperty, value);
            }
        }
        public static readonly DependencyProperty PromptProperty =
           DependencyProperty.Register(nameof(Prompt), typeof(string),
           typeof(SpecialFileControl), new PropertyMetadata("File:"));
        public string Prompt
        {
            get
            {
                return (string)GetValue(PromptProperty);
            }
            set
            {
                this.SetValue(PromptProperty, value);
            }
        }

        public static readonly DependencyProperty FilenameProperty =
          DependencyProperty.Register(nameof(Filename), typeof(string),
          typeof(SpecialFileControl), new PropertyMetadata(OnFilenamePropertyChanged));

        private static void OnFilenamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SpecialFileControl me)
            {
                if (!string.IsNullOrEmpty(me.Filename))
                {
                    FileInfo f = new FileInfo(me.Filename);
                    if (!me.IsSetting)
                    {
                        me.IsSetting = true;

                        //me.FileFolderPrefix
                        //TODO: If Base part of Filename is NOT FileFolderPrefix, then copy the file to
                        //     ArtemisActiveFolder + RelativeFolderPath that's already set.
                        //Note: NOTHING should ever go into base Artemis folder.  Everything should
                        // be under art or dat.
                        //Therefore, RelativeFolderPath MUST start with either "art" or "dat".  Can include more, though.
                        me.File = f.Name;


                        me.Value = me.RelativeFolderPath + "/" + me.File;
                        me.IsSetting = false;
                    }
                    if (f != null && !string.IsNullOrEmpty(f.DirectoryName) && f.Exists)
                    {
                        try
                        {
                            me.Image = new BitmapImage(new Uri(me.Filename));
                            me.IsImage = true;
                        }
                        catch (NotSupportedException)
                        {
                            me.IsImage = false;
                        }
                    }
                    me.IsSound = IsAudioFile(me.Filename);
                }
            }
        }

        public string Filename
        {
            get
            {
                return (string)GetValue(FilenameProperty);
            }
            set
            {
                this.SetValue(FilenameProperty, value);
            }
        }

        private void OnShowImage(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new(this.Filename);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);
        }
        System.Media.SoundPlayer? player = null;
        private void OnPlaySound(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsPlaying && player != null)
                {
                    player.Stop();
                    player = null;
                    IsPlaying = false;
                }
                else
                {
                    player = new System.Media.SoundPlayer();  //Can only play *.wav files.
                    player.SoundLocation = Filename;
                    player.Load();
                    player.Play();
                }
            }
            catch
            {

            }
        }
        public static bool IsAudioFile(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath);
            string[] audioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".aac", ".wma", ".m4a" }; // Add more if needed

            return audioExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }
    }
}
