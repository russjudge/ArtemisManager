using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerUI
{
    public class FileListItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string originalName = string.Empty;
        private bool isEditMode = false;
        public FileListItem(string name)
        {
            Name = name;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void DoChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                DoChanged();
                if (string.IsNullOrEmpty(OriginalName))
                {
                    OriginalName = value;
                }
            }
        }
        public string OriginalName
        {
            get { return originalName; }
            set
            {
                originalName = value;
                DoChanged();
            }
        }
        public bool IsEditMode
        {
            get { return isEditMode; }
            set
            {
                isEditMode = value;
                DoChanged();
            }
        }
    }
}
