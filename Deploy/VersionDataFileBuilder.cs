using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Deploy
{
    internal static class VersionDataFileBuilder
    {
        /// <summary>
        /// Creates .version file, copies setup file to target  NOT->, and copies all outdir files to target path/apptitle.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="appTitle"></param>
        /// <param name="setupFileNameFullPath"></param>
        /// <param name="sourceOutDir"></param>
        /// <param name="targetPath"></param>
        public static void BuildDataFile(string version, string appTitle, string setupFileNameFullPath, string sourceOutDir, string targetPath)
        {
            FileInfo setupFile = new FileInfo(setupFileNameFullPath);
            if (setupFile.Exists)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(targetPath, appTitle.Replace(" ", string.Empty) + ".version")))
                {
                    sw.WriteLine(version);
                    sw.WriteLine(setupFile.Name);
                }
                setupFile.CopyTo(targetPath);
            }
            //CopyFolder(sourceOutDir, Path.Combine(targetPath, appTitle.Replace(" ", string.Empty)));
        }
        
        public static void CopyFolder(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }
            if (!string.IsNullOrEmpty(sourceFolder) && Directory.Exists(sourceFolder) && !string.IsNullOrEmpty(targetFolder) && Directory.Exists(targetFolder))
            {
                foreach (var dir in new DirectoryInfo(sourceFolder).GetDirectories())
                {
                    CopyFolder(dir.FullName, Path.Combine(targetFolder, dir.Name));
                }
                foreach (var fle in new DirectoryInfo(sourceFolder).GetFiles())
                {
                    fle.CopyTo(targetFolder);
                }
            }
        }
        
        public static Tuple<string,string> GetVersion(string projectFile)
        {
            string version;
            string title;
            const string titleMatch = "<Title>";
            const string match = "<AssemblyVersion>";
            string projectData;
            using (StreamReader sr = new StreamReader(projectFile))
            {
                projectData= sr.ReadToEnd();
            }
            int i = projectData.IndexOf(match) + match.Length;
            if (i > -1)
            {
                int j = projectData.IndexOf('<', i);
                version = projectData.Substring(i, j - i);
            }
            else
            {
                version = "0.0.0.0";
            }
            i = projectData.IndexOf(titleMatch);
            if (i >= titleMatch.Length)
            {
                int j = projectData.IndexOf('<', i);
                title = projectData.Substring(i, j - i);
            }
            else
            {
                title = string.Empty;
            }
            return new Tuple<string, string>(version, title);
        }
    }
}
