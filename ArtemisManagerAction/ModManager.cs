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

        public static readonly string ModInstallFolder = Path.Combine(DataFolder, "InstalledMods");
        public static readonly string ModArchiveFolder = Path.Combine(DataFolder, "Archive");
        public static void InstallMod(string packagedFile, ModItem mod)
        {
            FileInfo package = new FileInfo(packagedFile);
            if (package.Exists)
            {
                string targetPath = Path.Combine(ModInstallFolder, mod.LocalIdentifier.ToString());
                if (Directory.Exists(targetPath))
                {
                    //foreach ()
                }

            }
        }
        public static void Unpackage(string file, string targetPath)
        {
            using (Stream stream = File.OpenRead(file))
            using (var reader = ReaderFactory.Open(stream))
            {
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
        }
    }
}
