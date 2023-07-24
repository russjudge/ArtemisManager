using ArtemisManagerAction;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for ModInstallWindow.xaml
    /// </summary>
    public partial class ModInstallWindow : Window
    {
        public ModInstallWindow()
        {
            Mod = new();
            InitializeComponent();
        }
        public static readonly DependencyProperty ForInstallProperty =
          DependencyProperty.Register(nameof(ForInstall), typeof(bool),
          typeof(ModInstallWindow));

        public bool ForInstall
        {
            get
            {

                return (bool)GetValue(ForInstallProperty);
            }
            set
            {
                this.SetValue(ForInstallProperty, value);
            }
        }

        public static readonly DependencyProperty ModProperty =
           DependencyProperty.Register(nameof(Mod), typeof(ModItem),
           typeof(ModInstallWindow));

        public ModItem Mod
        {
            get
            {

                return (ModItem)GetValue(ModProperty);
            }
            set
            {
                this.SetValue(ModProperty, value);
            }
        }
        public static readonly DependencyProperty PackageFileProperty =
          DependencyProperty.Register(nameof(PackageFile), typeof(string),
          typeof(ModInstallWindow), new PropertyMetadata(OnPackageFileChanged));

        private static void OnPackageFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ModInstallWindow me)
            {
                
                if (me.ForInstall)
                {
                    bool IsMission = true;
                    if (!string.IsNullOrEmpty(me.PackageFile) && System.IO.File.Exists(me.PackageFile))
                    {
                        //look for .json file to load data for.
                        using Stream stream = File.OpenRead(me.PackageFile);
                        using var reader = ReaderFactory.Open(stream);
                        while (reader.MoveToNextEntry())
                        {
                            if (reader.Entry.Key.StartsWith("dat/"))
                            {
                                IsMission = false;
                            }
                            if (!reader.Entry.IsDirectory && reader.Entry.Key.EndsWith(ArtemisManager.SaveFileExtension))
                            {
                               
                                int bytesRead = 0;
                                byte[] buffer = new byte[1024];
                                List<byte> bufList = new List<byte>();

                                using EntryStream entryStream = reader.OpenEntryStream();
                                if (entryStream != null)
                                {
                                    while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        bufList.AddRange(buffer);
                                    }
                                    string? data = System.Text.ASCIIEncoding.ASCII.GetString(bufList.ToArray());
                                    if (!string.IsNullOrEmpty(data))
                                    {
                                        try
                                        {
                                            ModItem? mod = ModItem.GetModItem(data);
                                            if (mod != null)
                                            {
                                                me.Mod.Author = mod.Author;
                                                me.Mod.CompatibleArtemisVersions = mod.CompatibleArtemisVersions;
                                                me.Mod.Description = mod.Description;
                                                me.Mod.InstallFolder = mod.InstallFolder;
                                                me.Mod.IsArtemisBase = mod.IsArtemisBase;
                                                me.Mod.Key = mod.Key;
                                                me.Mod.LocalIdentifier = mod.LocalIdentifier;
                                                me.Mod.Name = mod.Name;
                                                //me.Mod.PackageFile = mod.PackageFile;
                                                me.Mod.ReleaseDate = mod.ReleaseDate;
                                                me.Mod.RequiredArtemisVersion = mod.RequiredArtemisVersion;
                                                // me.Mod.SaveFile = mod.SaveFile;
                                                me.Mod.Version = mod.Version;
                                                
                                                break;
                                            }
                                        }
                                        catch
                                        {

                                        }

                                    }
                                }

                            }
                        }
                        me.Mod.IsMission = IsMission;
                    }
                }
                if (string.IsNullOrEmpty(me.Mod.Name) && !string.IsNullOrEmpty(me.PackageFile))
                {
                    string wrkName = new FileInfo(me.PackageFile).Name;
                    int i = wrkName.IndexOf('.');
                    if (i < 0)
                    {
                        i = wrkName.Length;
                    }

                    me.Mod.Name = wrkName.Substring(0, i).Replace("_", " ");
                    if (me.Mod.Name[0] >= 'A' && me.Mod.Name[0] <= 'Z'
                        && (me.Mod.Name[1] < 'A' || me.Mod.Name[1] > 'Z'))
                    {
                        StringBuilder sb = new();
                        sb.Append(me.Mod.Name[0]);

                        for (int j = 1; j < me.Mod.Name.Length; j++)
                        {
                            if (me.Mod.Name[j] >= 'A' && me.Mod.Name[j] <= 'Z')
                            {
                                sb.Append(' ');
                            }
                            sb.Append(me.Mod.Name[j]);
                        }
                        me.Mod.Name = sb.ToString();
                    }
                    if (me.Mod.Name.StartsWith("MISS "))
                    {
                        me.Mod.Name = me.Mod.Name.Substring(5);
                    }

                }
            }
        }

        public string PackageFile
        {
            get
            {

                return (string)GetValue(PackageFileProperty);
            }
            set
            {
                this.SetValue(PackageFileProperty, value);
            }
        }

        private void OnInstallMod(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PackageFile) && System.IO.File.Exists(PackageFile))
            {
                ModManager.InstallMod(PackageFile, Mod);
                TakeAction.SendClientInfo(IPAddress.Any);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a package file.");
            }
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {

            this.DialogResult = false;
            this.Close();
        }

        private void OnGenerateMod(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(PackageFile))
            {
                Mod.PackageFile = PackageFile;
                if (ModManager.GeneratePackage(Mod))
                {
                    ModManager.InstallMod(Mod.PackageFile, Mod);
                    TakeAction.SendClientInfo(IPAddress.Any);
                    this.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Cannot create Mod. No modifications found in the current active folder.\r\nCreate your mod at:\r\n" + ModItem.ActivatedFolder);
                    this.DialogResult = false;
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a package file.");
            }
        }
    }
}
