﻿using SharpCompress;
using System;
using System.Collections.Generic;
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
using WindowsAPICodePack.Dialogs;

namespace ArtemisManagerUI
{
    /// <summary>
    /// Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileSelector : UserControl
    {
        public FileSelector()
        {
            ButtonToolTip = "Search for file";
            InitializeComponent();
        }
        public static readonly DependencyProperty TextBoxLeftMarginProperty =
            DependencyProperty.Register(nameof(TextBoxLeftMargin), typeof(double),
            typeof(FileSelector));
        /// <summary>
        /// For aligning multiple FileSelector controls.
        /// </summary>
        public double TextBoxLeftMargin
        {
            get
            {

                return (double)GetValue(TextBoxLeftMarginProperty);
            }
            set
            {
                this.SetValue(TextBoxLeftMarginProperty, value);
            }
        }
        public static readonly DependencyProperty ButtonToolTipProperty =
            DependencyProperty.Register(nameof(ButtonToolTip), typeof(string),
            typeof(FileSelector));
        private string ButtonToolTip
        {
            get
            {
                return (string)GetValue(ButtonToolTipProperty);
            }
            set
            {
                this.SetValue(ButtonToolTipProperty, value);
            }
        }

        public static readonly DependencyProperty ShowButtonOnlyProperty =
           DependencyProperty.Register(nameof(ShowButtonOnly), typeof(bool),
           typeof(FileSelector));
        public bool ShowButtonOnly
        {
            get
            {
                return (bool)GetValue(ShowButtonOnlyProperty);
            }
            set
            {
                this.SetValue(ShowButtonOnlyProperty, value);
            }
        }


        public static readonly DependencyProperty FilterProperty =
           DependencyProperty.Register(nameof(Filter), typeof(string),
           typeof(FileSelector));
        public string? Filter
        {
            get
            {
                return (string?)GetValue(FilterProperty);
            }
            set
            {
                this.SetValue(FilterProperty, value);
            }
        }

        public static readonly DependencyProperty TitleProperty =
           DependencyProperty.Register(nameof(Title), typeof(string),
           typeof(FileSelector), new PropertyMetadata("File:"));
        public string Title
        {
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                this.SetValue(TitleProperty, value);
            }
        }
        public static readonly DependencyProperty IsFolderPickerProperty =
            DependencyProperty.Register(nameof(IsFolderPicker), typeof(bool),
            typeof(FileSelector), new PropertyMetadata(OnIsFolderPickerChanged));

        private static void OnIsFolderPickerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileSelector me)
            {
                me.ButtonToolTip = me.IsFolderPicker ? "Search for folder" : "Search for file";
            }

        }

        public bool IsFolderPicker
        {
            get
            {
                return (bool)GetValue(IsFolderPickerProperty);
            }
            set
            {
                this.SetValue(IsFolderPickerProperty, value);
            }
        }


        public static readonly DependencyProperty SelectedItemProperty =
          DependencyProperty.Register(nameof(SelectedItem), typeof(string),
          typeof(FileSelector));

        public string SelectedItem
        {
            get
            {
                return (string)GetValue(SelectedItemProperty);
            }
            set
            {
                this.SetValue(SelectedItemProperty, value);
            }
        }

        private void OnBrowse(object sender, RoutedEventArgs e)
        {
            string? initialDirectory = null;
            if (!string.IsNullOrEmpty(SelectedItem))
            {
                initialDirectory = new System.IO.FileInfo(SelectedItem).DirectoryName;
            }
            List<Tuple<string, string>>? filter = [];
            if (!string.IsNullOrEmpty(Filter))
            {
                string[] filters = Filter.Split('|');
                for (int i = 0; i < filters.Length; i += 2)
                {
                    if (i + 1 < filters.Length)
                    {
                        filter.Add(new(filters[i], filters[i + 1]));
                    }
                }
                if (filter.Count == 0)
                {
                    filter = null;
                }
            }
            string? value = BrowseForOneFile(Title, IsFolderPicker, initialDirectory, filter);
            if (!string.IsNullOrEmpty(value))
            {
                SelectedItem = value;
            }
        }
        public static string? BrowseForOneFile(
          string title,
          bool isFolderPicker,
          string? initialDirectory)
        {
            return BrowseForOneFile(title, isFolderPicker, initialDirectory, null);
        }

        public static string? BrowseForOneFile(
            string title,
            bool isFolderPicker,
            string? initialDirectory,
            IEnumerable<Tuple<string, string>>? filters
        )
        {
            string? retVal;
            var dlg = new CommonOpenFileDialog
            {
                Title = title,
                IsFolderPicker = isFolderPicker
            };
            if (filters != null)
            {
                foreach (Tuple<string, string> filter in filters)
                {
                    dlg.Filters.Add(new CommonFileDialogFilter(filter.Item1, filter.Item2));
                }
            }
            if (!string.IsNullOrEmpty(initialDirectory))
            {
                dlg.InitialDirectory = initialDirectory;
                dlg.DefaultDirectory = initialDirectory;
            }
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;

            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                retVal = dlg.FileName;
            }
            else
            {
                retVal = null;
            }
            return retVal;
        }
    }
}