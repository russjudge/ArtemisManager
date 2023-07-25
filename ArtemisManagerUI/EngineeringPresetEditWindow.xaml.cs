using AMCommunicator;
using ArtemisEngineeringPresets;
using ArtemisManagerAction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(".dat") )
            {
                Dispatcher.Invoke(() =>
                {
                    if (!PresetFiles.Contains(e.Name))
                    {
                        PresetFiles.Add(e.Name);
                    }
                });
            }
        }

        FileSystemWatcher? watcher;
        public static readonly DependencyProperty SelectedTargetPCProperty =
        DependencyProperty.Register(nameof(SelectedTargetPC), typeof(PCItem),
        typeof(EngineeringPresetEditWindow));

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
          typeof(EngineeringPresetEditWindow));

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
          typeof(EngineeringPresetEditWindow));

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
         typeof(EngineeringPresetEditWindow), new PropertyMetadata(OnSelectedPresetFileChanged));

        private static void OnSelectedPresetFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetEditWindow me)
            {
                if (!string.IsNullOrEmpty(me.SelectedPresetFile) && System.IO.File.Exists(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, me.SelectedPresetFile)))
                {
                    bool IsOkay = true;

                    if (me.SelectedFile != null && me.SelectedFile.HasChanges())
                    {
                        switch (System.Windows.MessageBox.Show("Save changes to current Presets File?", "Presets File changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:
                                me.SelectedFile.Save();
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
                        me.SelectedFile = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder,me.SelectedPresetFile));
                    }
                    else
                    {
                        if (me.SelectedFile != null && !string.IsNullOrEmpty(me.SelectedFile.SaveFile))
                        {
                            me.SelectedPresetFile = new System.IO.FileInfo(me.SelectedFile.SaveFile).Name;
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
          typeof(EngineeringPresetEditWindow));

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
                    string source = System.IO.Path.Combine(ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder, ArtemisManagerAction.ArtemisManager.OriginalArtemisEngineeringFile);
                    if (File.Exists(source))
                    {
                        System.IO.File.Copy(
                            source,
                            dialg.FileName);
                    }
                    else
                    {
                        PresetsFile f = new PresetsFile();
                        f.SaveFile = dialg.FileName;
                        f.Save();
                    }
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
