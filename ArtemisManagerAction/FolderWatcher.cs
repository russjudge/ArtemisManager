using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class FolderWatcher : IDisposable
    {
        static FolderWatcher()
        {
            Watchers.Add(ArtemisManager.ArtemisINIFolder, new(ArtemisManager.ArtemisINIFolder));
            Watchers.Add(ArtemisManager.EngineeringPresetsFolder, new(ArtemisManager.EngineeringPresetsFolder));
        }
        private FileSystemWatcher watcher;
        private bool disposedValue;

        private FolderWatcher(string folder)
        {
            watcher = new FileSystemWatcher(folder);
            watcher.EnableRaisingEvents = true;
        }
        public static Dictionary<string, FolderWatcher> Watchers { get; private set; } = new();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                watcher.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FolderWatcher()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
