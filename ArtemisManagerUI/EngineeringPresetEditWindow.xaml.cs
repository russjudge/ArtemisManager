using AMCommunicator;
using ArtemisEngineeringPresets;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for EngineeringPresetEditWindow.xaml
    /// </summary>
    public partial class EngineeringPresetEditWindow : Window
    {
        public EngineeringPresetEditWindow()
        {
            PresetFiles = new ObservableCollection<string>(ArtemisManagerAction.ArtemisManager.GetEngineeringPresetFiles());
            InitializeComponent();
            ModManager.CreateFolder(ArtemisManager.EngineeringPresetsFolder);
            watcher = new FileSystemWatcher(ArtemisManager.EngineeringPresetsFolder);
            watcher.Created += Watcher_Created;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(".dat"))
            {
                PresetFiles.Add(e.Name);
            }
        }

        FileSystemWatcher? watcher;
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
        typeof(PresetSettingsControl));

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

        public static readonly DependencyProperty ConnectedPCsProperty =
          DependencyProperty.Register(nameof(ConnectedPCs), typeof(ObservableCollection<PCItem>),
          typeof(PresetSettingsControl));

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

        public static readonly DependencyProperty PresetFilesProperty =
          DependencyProperty.Register(nameof(PresetFiles), typeof(ObservableCollection<string>),
          typeof(PresetSettingsControl));

        public ObservableCollection<string> PresetFiles
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(PresetFilesProperty);
            }
            set
            {
                this.SetValue(PresetFilesProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedPresetFileProperty =
         DependencyProperty.Register(nameof(SelectedPresetFile), typeof(string),
         typeof(PresetSettingsControl), new PropertyMetadata(OnSelectedPresetFileChanged));

        private static void OnSelectedPresetFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetEditWindow window)
            {
                if (!string.IsNullOrEmpty(window.SelectedPresetFile) && System.IO.File.Exists(window.SelectedPresetFile))
                {
                    bool IsOkay = true;
                    if (window.SelectedFile != null && window.SelectedFile.HasChanges())
                    {
                        switch (System.Windows.MessageBox.Show("Save changes to current Presets File?", "Presets File changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                window.SelectedFile.Save();
                                IsOkay = true;
                                break;
                            case MessageBoxResult.No:
                                IsOkay = true;
                                break;
                            case MessageBoxResult.Cancel:
                                IsOkay = false;
                                break;
                        }
                    }
                    if (IsOkay)
                    {
                        window.SelectedFile = new(window.SelectedPresetFile);
                    }
                    else
                    {
                        if (window.SelectedFile != null && !string.IsNullOrEmpty(window.SelectedFile.SaveFile))
                        {
                            window.SelectedPresetFile = new System.IO.FileInfo(window.SelectedFile.SaveFile).Name;
                        }
                    }
                }
            }
        }

        public string SelectedPresetFile
        {
            get
            {
                return (string)this.GetValue(SelectedPresetFileProperty);
            }
            set
            {
                this.SetValue(SelectedPresetFileProperty, value);
            }
        }


        public static readonly DependencyProperty SelectedFileProperty =
          DependencyProperty.Register(nameof(SelectedFile), typeof(PresetsFile),
          typeof(PresetSettingsControl));

        public PresetsFile? SelectedFile
        {
            get
            {
                return (PresetsFile?)this.GetValue(SelectedFileProperty);
            }
            set
            {
                this.SetValue(SelectedFileProperty, value);
            }
        }

        private void OnAddPresetFile(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog dialg = new())
            {
                dialg.CheckPathExists = true;

                dialg.Filter = "Engineering Presets (*.dat)|*.dat";
                dialg.DefaultExt = "dat";
                dialg.InitialDirectory = ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder;
                dialg.Title = "Select new presets filename";
                if (dialg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.File.Copy(
                        System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder, ArtemisManagerAction.ArtemisManager.OriginalArtemisEngineeringFile),
                        dialg.FileName);
                    PresetFiles.Add(dialg.FileName);
                    SelectedFile = new PresetsFile(dialg.FileName);
                }
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button me)
            {
                if (me.CommandParameter is string filename )
                {
                    string fullname = System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder, filename);
                    if (System.IO.File.Exists(fullname))
                    {
                        System.IO.File.Delete(fullname);
                    }
                    if (PresetFiles.Contains(filename))
                    {
                        PresetFiles.Remove(filename);
                    }
                    if (SelectedFile?.SaveFile == fullname)
                    {
                        SelectedFile = null;
                    }
                }
            }
        }

        private void OnSendSelectedFile(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && SelectedTargetPC != null && SelectedTargetPC.IP != null)
            {
                if (SelectedTargetPC.IP.ToString() == IPAddress.Any.ToString())
                {
                    foreach (var pcItem in ConnectedPCs)
                    {
                        if (pcItem.IP != null && pcItem.IP.ToString() != IPAddress.Any.ToString())
                        {
                            Network.Current?.SendJsonPackageFile(pcItem.IP, SelectedFile.GetJSON(), AMCommunicator.Messages.JsonPackageFile.EngineeringPreset, SelectedFile.SaveFile);
                        }
                    }
                }
                else
                {
                    Network.Current?.SendJsonPackageFile(SelectedTargetPC.IP, SelectedFile.GetJSON(), AMCommunicator.Messages.JsonPackageFile.EngineeringPreset, SelectedFile.SaveFile);
                }
            }
        }
    }
}
