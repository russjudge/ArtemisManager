using Microsoft.Win32;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace ArtemisManagerAction
{
    public class ModManager
    {
        public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Confederate In Blue Gaming", "Artemis Manager");

        
        public static readonly string ModArchiveFolder = Path.Combine(DataFolder, "Archive");
        public static bool IsModInstalled(ModItem mod)
        {
            bool found = false;
            foreach (var modItem in ArtemisManager.GetInstalledMods() )
            {
                if (modItem.LocalIdentifier.Equals(mod.LocalIdentifier)
                    || modItem.LocalIdentifier.Equals(mod.ModIdentifier)
                    || modItem.ModIdentifier.Equals(mod.LocalIdentifier)
                    || modItem.ModIdentifier.Equals(mod.ModIdentifier))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        
        public static void InstallMod(string packagedFile, ModItem mod)
        {
            FileInfo package = new(packagedFile);
            if (package.Exists && !IsModInstalled(mod))
            {
                string targetPath = Path.Combine(ModItem.ModInstallFolder, mod.GetSaveFile());
                if (Directory.Exists(targetPath))
                {
                    Unpackage(packagedFile, targetPath);
                }
                mod.PackageFile = package.Name;
                mod.Save();
                File.Copy(packagedFile, Path.Combine(ModArchiveFolder, package.Name));
            }
        }
        private static void Unpackage(string file, string targetPath)
        {
            using Stream stream = File.OpenRead(file);
            using var reader = ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    reader.WriteEntryToDirectory(targetPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }
        public static void CreateFolder(string? path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static void CopyFolder(string sourceFolder, string destinationFolder)
        {
            CreateFolder(destinationFolder);
            foreach (var directory in new DirectoryInfo(sourceFolder).GetDirectories())
            {
                CopyFolder(directory.FullName, Path.Combine(destinationFolder, directory.Name));
            }
            foreach (var fle in new DirectoryInfo(sourceFolder).GetFiles())
            {
                fle.CopyTo(Path.Combine(destinationFolder, fle.Name));
            }
        }
    }
}
