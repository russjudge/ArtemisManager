using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
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
        public static bool IsModInstalled(ModItem? mod)
        {
            if (mod == null) return false;
            bool found = false;
            foreach (var modItem in ArtemisManager.GetInstalledMods() )
            {
                if (modItem.LocalIdentifier.Equals(mod.LocalIdentifier)
                    || modItem.LocalIdentifier.Equals(mod.ModIdentifier)
                    || (!modItem.ModIdentifier.Equals(Guid.Empty) 
                    && (modItem.ModIdentifier.Equals(mod.LocalIdentifier)
                    || modItem.ModIdentifier.Equals(mod.ModIdentifier)))
                    || (!string.IsNullOrEmpty(mod.PackageFile) && modItem.PackageFile.Equals(mod.PackageFile)))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
        public static bool GeneratePackage(ModItem mod)
        {
            bool retval = false;
            var activeArtemisVersion = ArtemisManagerAction.ArtemisManager.GetArtemisVersion(ModItem.ActivatedFolder);
            ModItem? baseArtemisMod = null;
            foreach (var modItem in ArtemisManager.GetInstalledMods())
            {
                if (modItem.IsArtemisBase && modItem.Version == activeArtemisVersion)
                {
                    baseArtemisMod = modItem;
                    break;
                }
            }
            if (baseArtemisMod != null)
            {
                var baseArtemisFiles = GetFiles(Path.Combine(ModItem.ModInstallFolder, baseArtemisMod.InstallFolder));
                var newModFiles = GetFiles(ModItem.ActivatedFolder);
                List<string> workFiles = new();
                foreach (var file in newModFiles)
                {
                    workFiles.Add(file.Replace(ModItem.ActivatedFolder, string.Empty));
                }
                foreach (var file in baseArtemisFiles)
                {
                    string matchFile = file.Replace(Path.Combine(ModItem.ModInstallFolder, baseArtemisMod.InstallFolder), string.Empty);
                    if (workFiles.Contains(matchFile))
                    {
                        if (!file.EndsWith(".exe") && !file.EndsWith(".dll"))
                        {
                            if (file.EndsWith(".ini") || file.EndsWith(".xml"))
                            {
                                string baseData;
                                string modData;
                                using (StreamReader sr = new(file))
                                {
                                    baseData = sr.ReadToEnd();
                                }
                                using (StreamReader sr = new(Path.Combine(ModItem.ActivatedFolder, matchFile)))
                                {
                                    modData = sr.ReadToEnd();
                                }
                                if (baseData.Equals(modData))
                                {
                                    workFiles.Remove(matchFile);
                                }
                            }
                            else
                            {
                                bool fullMatch = true;
                                byte[] baseData = new byte[32768];
                                byte[] modData = new byte[32768];
                                int bytesReadBase = 0;
                                int bytesReadMod = 0;
                                using (FileStream fsBase = new(file, FileMode.Open))
                                {
                                    using FileStream fsMod = new(Path.Combine(ModItem.ActivatedFolder, matchFile), FileMode.Open);
                                    while ((bytesReadBase = fsBase.Read(baseData, 0, baseData.Length)) > 0)
                                    {
                                        int totalModBytes = 0;
                                        bytesReadMod = fsMod.Read(modData, 0, bytesReadBase);
                                        totalModBytes += bytesReadMod;
                                        while (totalModBytes < bytesReadBase)
                                        {
                                            bytesReadMod = fsMod.Read(modData, totalModBytes, bytesReadBase - totalModBytes);
                                        }
                                        for (int i = 0; i < bytesReadBase; i++)
                                        {
                                            if (modData[i] != baseData[i])
                                            {
                                                fullMatch = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (fullMatch)
                                {
                                    workFiles.Remove(matchFile);
                                }
                            }
                        }
                        else
                        {
                            workFiles.Remove(matchFile);
                        }
                    }
                }
                for (int i = 0; i < workFiles.Count; i++)
                {
                    if (workFiles[i].StartsWith("\\") || workFiles[i].StartsWith("/"))
                    {
                        workFiles[i] = workFiles[i].Substring(1);
                    }
                }
                if (workFiles.Count > 0)
                {
                    retval = true;
                    if (File.Exists(mod.PackageFile))
                    {
                        File.Delete(mod.PackageFile);
                    }

                    using SharpCompress.Archives.Zip.ZipArchive? archive = SharpCompress.Archives.Zip.ZipArchive.Create();

                    foreach (var file in workFiles)
                    {
                        archive?.AddEntry(file, Path.Combine(ModItem.ActivatedFolder, file));
                    }
                    using (MemoryStream ms = new(Encoding.ASCII.GetBytes(mod.GetJSON())))
                    {
                        ms.Position = 0;
                        archive.AddEntry(mod.SaveFile, ms);
                    }
                    var options = new SharpCompress.Writers.WriterOptions(CompressionType.Deflate);
                    archive?.SaveTo(mod.PackageFile, options);
                }
            }
            if (retval)
            {
                mod.ModIdentifier = Guid.NewGuid();
            }
            return retval;
        }
        private static string[] GetFiles(string rootPath)
        {
            List<string> retVal = new();
            foreach (var dir in new DirectoryInfo(rootPath).GetDirectories())
            {
                retVal.AddRange(GetFiles(dir.FullName));
            }
            foreach (var file in new DirectoryInfo(rootPath).GetFiles())
            {
                retVal.Add(file.FullName);
            }
            return retVal.ToArray();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="packagedFile">Full path to compressed package file.</param>
        /// <param name="mod"></param>
        public static void InstallMod(string packagedFile, ModItem? mod)
        {
            if (mod != null)
            {
                FileInfo package = new(packagedFile);
                if (package.Exists && !IsModInstalled(mod))
                {
                    ModManager.CreateFolder(ModArchiveFolder);
                    
                    File.Copy(packagedFile, Path.Combine(ModArchiveFolder, package.Name), true);
                    mod.PackageFile = package.Name;
                    mod.Save();
                    mod.Unpack();
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
                fle.CopyTo(Path.Combine(destinationFolder, fle.Name), true);
            }
        }
    }
}
