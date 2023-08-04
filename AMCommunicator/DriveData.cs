using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMCommunicator
{
    public class DriveData
    {
        public DriveData(DriveInfo drive)
        {
            Name = drive.Name;
            try
            {
                FreeSpace = drive.AvailableFreeSpace;
            }
            catch { }
            try
            {
                TotalSize = drive.TotalSize;
            }
            catch { }
            try
            {
                DriveFormat = drive.DriveFormat;
            }
            catch
            {
                DriveFormat = "Unknown";
            }
            try
            {
                DriveType = drive.DriveType;
            }
            catch { }
            IsAppDrive = (drive.Name.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)[..1]));
        }
        public string Name { get; private set; }
        public long FreeSpace { get; private set; }
        public bool IsAppDrive { get; private set; }
        public long TotalSize { get; private set; }
        public string DriveFormat { get; private set; }
        public DriveType DriveType { get; private set; }

        public static DriveData[] GetDriveData()
        {
            List<DriveData> retVal = new();
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                retVal.Add(new(drive));
            }
            return retVal.ToArray();
        }
    }
}
