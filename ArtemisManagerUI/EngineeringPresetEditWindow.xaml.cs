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

       
       
        public static readonly DependencyProperty PopupMessageProperty =
         DependencyProperty.Register(nameof(PopupMessage), typeof(string),
             typeof(EngineeringPresetEditWindow));

        public string PopupMessage
        {
            get
            {
                return (string)this.GetValue(PopupMessageProperty);

            }
            set
            {
                this.SetValue(PopupMessageProperty, value);

            }
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
                    PresetsFile f = new()
                    {
                        SaveFile = dialg.FileName
                    };
                    f.Save();
                    SelectedFile = new PresetsFile(dialg.FileName);
                }
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PresetsFile presetsFile && !string.IsNullOrEmpty(presetsFile?.SaveFile) && System.IO.File.Exists(presetsFile?.SaveFile))
            {
                string nm = new FileInfo(presetsFile.SaveFile).Name;
                string fullname = presetsFile.SaveFile;
                if (System.Windows.MessageBox.Show(string.Format("Are you sure you want to delete {0}", nm), "Delete Presets file",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    presetsFile.Delete();
                    if (PresetFiles.Contains(nm))
                    {
                        PresetFiles.Remove(nm);
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
            if (SelectedFile != null && SelectedTargetPC != null && SelectedTargetPC.IP != null && !string.IsNullOrEmpty(SelectedFile.SaveFile) && System.IO.File.Exists(SelectedFile.SaveFile))
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

        private void OnActivate(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PresetsFile presetsFile && !string.IsNullOrEmpty(presetsFile?.SaveFile) && System.IO.File.Exists(presetsFile?.SaveFile))
            {
                File.Copy(presetsFile.SaveFile, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
            }
        }

        private void OnRestoreToDefault(object sender, RoutedEventArgs e)
        {
            System.IO.FileInfo fle = new(System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile));
            if (fle.Exists)
            {
                fle.Delete();
            }
            System.Windows.MessageBox.Show("Engieering Presets restored to defaults.");

        }

        private void OnExportPresets(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PresetsFile presetsFile)
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
                        bool OkayToSave = true;
                        if (File.Exists(dialg.FileName))
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + new FileInfo(dialg.FileName).Name + "?",
                                "Export Presets File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }
                        if (OkayToSave)
                        {
                            File.Copy(presetsFile.SaveFile, dialg.FileName, true);
                        }
                    }
                }
            }
        }

        private void OnImportPresets(object sender, RoutedEventArgs e)
        {
            bool OkayToSave = true;
            using OpenFileDialog dialg = new();
            dialg.CheckPathExists = true;
            dialg.CheckFileExists = true;
            dialg.Filter = "Engineering Presets (*.dat)|*.dat";
            dialg.DefaultExt = "dat";
            dialg.Title = "Select presets filename";
            if (dialg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileInfo source = new(dialg.FileName);
                PresetsFile? work = null;
                try
                {
                    work = new PresetsFile(source.FullName);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        "Invalid Engineering Presets file:\r\n\r\n" + ex.Message,
                        "Import Engineering Presets",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    OkayToSave = false;
                    
                }
                if (OkayToSave)
                {
                    if (work == null)
                    {
                        System.Windows.MessageBox.Show(
                       "Invalid Engineering Presets file.  Unable to process.",
                       "Import Engineering Presets",
                       MessageBoxButton.OK,
                       MessageBoxImage.Error);
                    }
                    else
                    {
                        FileInfo target = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, source.Name));
                        if (target.Exists)
                        {
                            OkayToSave = (System.Windows.MessageBox.Show("Do you want to overwrite " + source.Name + "?",
                                       "Import Presets File", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        }
                        if (OkayToSave)
                        {
                            source.CopyTo(target.FullName, true);
                            SelectedFile = work;
                        }
                    }
                }
            }
        }

        private void OnSaveSelectedPresets(object sender, RoutedEventArgs e)
        {
            SelectedFile?.Save();
        }

        private void OnActivateSelectedPresets(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !string.IsNullOrEmpty(SelectedFile.SaveFile) && System.IO.File.Exists(SelectedFile.SaveFile))
            {
                File.Copy(SelectedFile.SaveFile, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
                PopupMessage = "Presets Activated.";
            }
        }

        private void OnDeleteSelectedPresets(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && !string.IsNullOrEmpty(SelectedFile.SaveFile) && System.IO.File.Exists(SelectedFile.SaveFile))
            {
                var nm = new FileInfo(SelectedFile.SaveFile).Name;
                SelectedFile.Delete();
                if (PresetFiles.Contains(nm))
                {
                    PresetFiles.Remove(nm);
                }
                SelectedFile = null;
                
            }
        }

        private void OnSaved(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Presets Saved.";
        }
    }
}
