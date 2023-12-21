using ArtemisManagerAction;
using ArtemisManagerAction.ArtemisEngineeringPresets;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for EngineeringPresetEditWindow.xaml
    /// </summary>
    public partial class EngineeringPresetEditWindow : Window
    {
        public EngineeringPresetEditWindow()
        {
            PresetFiles = [];
            foreach (var name in ArtemisManagerAction.ArtemisManager.GetEngineeringPresetFiles())
            {
                PresetFiles.Add(new FileListItem(name.Substring(0, name.Length - 4)));
            }
            InitializeComponent();
            ModManager.CreateFolder(ArtemisManager.EngineeringPresetsFolder);
            watcher = new FileSystemWatcher(ArtemisManager.EngineeringPresetsFolder);
            watcher.Created += Watcher_Created;
            watcher.Deleted += Watcher_Deleted;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    FileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        PresetFiles.Remove(remover);
                    }
                });
            }
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
            if (!string.IsNullOrEmpty(e.Name) && e.Name.EndsWith(ArtemisManager.DATFileExtension))
            {
                Dispatcher.Invoke(() =>
                {
                    string match = e.Name.Substring(0, e.Name.Length - 4);
                    FileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == match)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover == null)
                    {
                        PresetFiles.Add(new(match));
                    }
                });
            }
        }

#pragma warning disable IDE0044 // Add readonly modifier
        FileSystemWatcher? watcher;
#pragma warning restore IDE0044 // Add readonly modifier


        public static readonly DependencyProperty PresetFilesProperty =
          DependencyProperty.Register(nameof(PresetFiles), typeof(ObservableCollection<FileListItem>),
          typeof(EngineeringPresetEditWindow));

        public ObservableCollection<FileListItem> PresetFiles
        {
            get
            {
                return (ObservableCollection<FileListItem>)this.GetValue(PresetFilesProperty);
            }
            set
            {
                this.SetValue(PresetFilesProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedPresetFileProperty =
         DependencyProperty.Register(nameof(SelectedPresetFile), typeof(FileListItem),
         typeof(EngineeringPresetEditWindow), new PropertyMetadata(OnSelectedPresetFileChanged));

        private static void OnSelectedPresetFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EngineeringPresetEditWindow me)
            {
                if (me.SelectedPresetFile != null && !string.IsNullOrEmpty(me.SelectedPresetFile.Name)
                    && System.IO.File.Exists(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, me.SelectedPresetFile.Name + ArtemisManager.DATFileExtension)))
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
                        me.SelectedFile = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, me.SelectedPresetFile.Name + ArtemisManager.DATFileExtension));
                    }
                    else
                    {
                        if (me.SelectedFile != null && !string.IsNullOrEmpty(me.SelectedFile.SaveFile))
                        {
                            var fle = new System.IO.FileInfo(me.SelectedFile.SaveFile);
                            me.SelectedPresetFile = new(fle.Name.Substring(0, fle.Name.Length - 4));
                        }
                    }
                }
            }
        }

        public FileListItem? SelectedPresetFile
        {
            get
            {
                return (FileListItem?)this.GetValue(SelectedPresetFileProperty);
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
            string newFile = "NewPresets";
            int i = 0;
            bool mustretry = true;
            while (mustretry)
            {
                mustretry = false;
                foreach (var item in PresetFiles)
                {
                    if (item.Name == newFile)
                    {
                        newFile = "NewPresets(" + (++i).ToString() + ")";
                        mustretry = true;
                        break;
                    }
                }
            }

            PresetsFile f = new()
            {
                SaveFile = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, newFile + ArtemisManager.DATFileExtension)
            };
            f.Initialize();
            f.Save();
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is PresetsFile presetsFile && !string.IsNullOrEmpty(presetsFile?.SaveFile) && System.IO.File.Exists(presetsFile?.SaveFile))
            {
                string nm = new FileInfo(presetsFile.SaveFile).Name.Substring(presetsFile.SaveFile.Length - 4);

                string fullname = presetsFile.SaveFile;
                if (System.Windows.MessageBox.Show(string.Format("Are you sure you want to delete {0}", nm), "Delete Presets file",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    presetsFile.Delete();

                    FileListItem? remover = null;
                    foreach (var item in PresetFiles)
                    {
                        if (item.Name == nm)
                        {
                            remover = item;
                            break;
                        }
                    }
                    if (remover != null)
                    {
                        PresetFiles.Remove(remover);
                    }
                    if (SelectedPresetFile?.Name == nm)
                    {
                        SelectedPresetFile = null;
                    }
                    if (SelectedFile?.SaveFile == fullname)
                    {
                        SelectedFile = null;
                    }
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
                SaveFileDialog dialg = new()
                {
                    CheckPathExists = true,
                    Filter = "Engineering Presets (*" + ArtemisManager.DATFileExtension + ")|*" + ArtemisManager.DATFileExtension,
                    DefaultExt = "dat",
                    InitialDirectory = ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder,
                    Title = "Select new presets filename"
                };
                if (dialg.ShowDialog() == true)
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

        private void OnImportPresets(object sender, RoutedEventArgs e)
        {
            bool OkayToSave = true;
            OpenFileDialog dialg = new()
            {
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "Engineering Presets (*" + ArtemisManager.DATFileExtension + ")|*" + ArtemisManager.DATFileExtension,
                DefaultExt = "dat",
                Title = "Select presets filename"
            };
            if (dialg.ShowDialog() == true)
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
                        string.Format("Invalid Engineering Presets file:{1}{1}{0}", ex.Message, Environment.NewLine),
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
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.DATFileExtension);
                        if (System.IO.File.Exists(source))
                        {
                            File.Copy(source, System.IO.Path.Combine(ModItem.ActivatedFolder, ArtemisManager.ArtemisEngineeringFile), true);
                            PopupMessage = "Presets Activated.";
                        }
                    }
                }
            }
        }

        private void OnDeleteSelectedPresets(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name))
                    {
                        PresetsFile pf = new(System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.DATFileExtension));
                        pf.Delete();
                        PresetFiles.Remove(selectedFile);
                        if (SelectedFile != null && SelectedFile.SaveFile == selectedFile.Name + ArtemisManager.DATFileExtension)
                        {
                            SelectedFile = null;
                        }
                        if (SelectedPresetFile == selectedFile)
                        {
                            SelectedPresetFile = null;
                        }
                    }
                }
            }
        }

        private void OnSaved(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Presets Saved.";
        }

        private void OnSendFileRequest(object sender, FileRequestRoutedEventArgs e)
        {
            e.File = SelectedFile;
        }

        private void OnTransmissionCompleted(object sender, RoutedEventArgs e)
        {
            PopupMessage = "Transmission completed.";
        }

        private void OnPresetRename(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem me)
            {
                if (me.CommandParameter is FileListItem selectedFile)
                {
                    selectedFile.IsEditMode = true;
                }
            }
        }

        private void OnSelectedFilenameTextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlockEditControl me)
            {
                if (me.Tag is FileListItem selectedFile)
                {
                    if (!string.IsNullOrEmpty(selectedFile.Name) && !string.IsNullOrEmpty(selectedFile.OriginalName) && selectedFile.Name != selectedFile.OriginalName)
                    {
                        string source = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.OriginalName + ArtemisManager.DATFileExtension);
                        string target = System.IO.Path.Combine(ArtemisManager.EngineeringPresetsFolder, selectedFile.Name + ArtemisManager.DATFileExtension);
                        if (File.Exists(target))
                        {
                            MessageBox.Show("Cannot rename--new name already exists.", "Rename presets file", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            File.Move(source, target, true);
                            selectedFile.OriginalName = selectedFile.Name;

                            if (SelectedFile != null && SelectedFile.SaveFile == selectedFile.OriginalName + ArtemisManager.DATFileExtension)
                            {
                                SelectedFile.SaveFile = selectedFile.Name + ArtemisManager.DATFileExtension;
                            }
                        }
                    }
                }
            }
        }
    }
}
