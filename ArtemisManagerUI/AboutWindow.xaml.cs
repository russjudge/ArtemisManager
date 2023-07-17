using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Application name property.
        /// </summary>
        public static readonly DependencyProperty ApplicationNameProperty =
            DependencyProperty.Register(
                nameof(ApplicationName),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Application Path property.
        /// </summary>
        public static readonly DependencyProperty ApplicationPathProperty =
            DependencyProperty.Register(
                nameof(ApplicationPath),
                typeof(string),
                typeof(AboutWindow));

        

        /// <summary>
        /// version property.
        /// </summary>
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register(
                nameof(Version),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Has Version History property.
        /// </summary>
        public static readonly DependencyProperty HasVersionHistoryProperty =
            DependencyProperty.Register(
                nameof(HasVersionHistory),
                typeof(bool),
                typeof(AboutWindow));

        /// <summary>
        /// Company property.
        /// </summary>
        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register(
                nameof(Company),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Company property.
        /// </summary>
        public static readonly DependencyProperty AuthorProperty =
            DependencyProperty.Register(
                nameof(Author),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Copyright property.
        /// </summary>
        public static readonly DependencyProperty CopyrightProperty =
            DependencyProperty.Register(
                nameof(Copyright),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Description property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(AboutWindow));

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutWindow"/> class.
        /// </summary>
        public AboutWindow()
        {
            
            var keyAssm = System.Reflection.Assembly.GetEntryAssembly();
            if (keyAssm != null)
            {
                this.ApplicationPath = keyAssm.Location.Replace(".dll", ".exe");
                var appName = keyAssm.GetName();
                this.ApplicationName = appName.FullName;
                if (appName != null && appName.Version != null)
                {
                    this.Version = appName.Version.ToString();
                }

                var comp = keyAssm.GetCustomAttribute<AssemblyCompanyAttribute>();
                if (comp != null)
                {
                    this.Company = comp.Company;
                }


                this.Author = "Russ Judge";
                

                var copyright = keyAssm.GetCustomAttribute<AssemblyCopyrightAttribute>();
                if (copyright != null)
                {
                    this.Copyright = copyright.Copyright;
                }

                var desc = keyAssm.GetCustomAttribute<AssemblyDescriptionAttribute>();
                if (desc != null)
                {
                    this.Description = desc.Description;
                }

                var title = keyAssm.GetCustomAttribute<AssemblyTitleAttribute>();

                if (title != null)
                {
                    this.ApplicationName = title.Title;
                }
            }

            if (this.ApplicationPath != null)
            {
                var fle = new System.IO.FileInfo(this.ApplicationPath);
                if (fle != null && fle.DirectoryName != null)
                {
                    string versionFile = System.IO.Path.Combine(fle.DirectoryName, "changes.txt");
                    this.HasVersionHistory = System.IO.File.Exists(versionFile);
                }
            }

            this.Title = "About " + this.ApplicationName;
            this.InitializeComponent();
        }

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets Application name.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return (string)this.GetValue(ApplicationNameProperty);
            }

            set
            {
                this.SetValue(ApplicationNameProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets application path.
        /// </summary>
        public string ApplicationPath
        {
            get
            {
                return (string)this.GetValue(ApplicationPathProperty);
            }

            set
            {
                this.SetValue(ApplicationPathProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Version.
        /// </summary>
        public string Version
        {
            get
            {
                return (string)this.GetValue(VersionProperty);
            }

            set
            {
                this.SetValue(VersionProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Has Version History.
        /// </summary>
        public bool HasVersionHistory
        {
            get
            {
                return (bool)this.GetValue(HasVersionHistoryProperty);
            }

            set
            {
                this.SetValue(HasVersionHistoryProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Company.
        /// </summary>
        public string Company
        {
            get
            {
                return (string)this.GetValue(CompanyProperty);
            }

            set
            {
                this.SetValue(CompanyProperty, value);
                this.NotifyPropertyChanged();
            }
        }
        public string Author
        {
            get
            {
                return (string)this.GetValue(AuthorProperty);
            }

            set
            {
                this.SetValue(AuthorProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// gets or sets Copyright.
        /// </summary>
        public string Copyright
        {
            get
            {
                return (string)this.GetValue(CopyrightProperty);
            }

            set
            {
                this.SetValue(CopyrightProperty, value);
                this.NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description
        {
            get
            {
                return (string)this.GetValue(DescriptionProperty);
            }

            set
            {
                this.SetValue(DescriptionProperty, value);
                this.NotifyPropertyChanged();
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnViewHistory(object sender, RoutedEventArgs e)
        {
            var assm = System.Reflection.Assembly.GetEntryAssembly();
            if (assm != null)
            {
                FileInfo f = new(assm.Location);
                if (!string.IsNullOrEmpty(f.DirectoryName))
                {
                    System.Diagnostics.Process.Start(System.IO.Path.Combine(f.DirectoryName, "changes.txt"));
                }
            }
        }
    }
}
