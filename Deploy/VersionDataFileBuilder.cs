using System;
using System.Collections.Generic;
using System.IO;
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
            FileInfo setupFile = new(setupFileNameFullPath);
            if (setupFile.Exists)
            {
                string versionFile = Path.Combine(targetPath, appTitle.Replace(" ", string.Empty).ToLowerInvariant() + ".version");
                if (File.Exists(versionFile))
                {
                    File.Delete(versionFile);
                }
                using (StreamWriter sw = new(versionFile))
                {
                    sw.WriteLine(version);
                    sw.WriteLine(setupFile.Name.ToLowerInvariant());
                }
                setupFile.CopyTo(Path.Combine(targetPath, setupFile.Name.ToLowerInvariant()), true);
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
                    fle.CopyTo(Path.Combine(targetFolder, fle.Name), true);
                }
            }
        }

        public static Tuple<string, string> GetVersion(string projectFile)
        {

            string projectData;
            using (StreamReader sr = new(projectFile))
            {
                projectData = sr.ReadToEnd();
            }

            return ProcessProjectData(projectData);
        }
        public static Tuple<string, string> ProcessProjectData(string projectData)
        {
            string version;
            string title;
            const string titleMatch = "<Title>";
            const string match = "<AssemblyVersion>";
            int i = projectData.IndexOf(match) + match.Length;
            if (i > match.Length)
            {
                int j = projectData.IndexOf('<', i);
                string ver = projectData.Substring(i, j - i);
                var parts = ver.Split('.');

                if (int.TryParse(parts[3], out int piece))
                {
                    piece--;
                    version = parts[0] + "." + parts[1] + "." + parts[2] + "." + piece.ToString();
                }
                else
                {
                    version = "0.0.0.0";
                }
            }
            else
            {
                version = "0.0.0.0";
            }
            i = projectData.IndexOf(titleMatch) + titleMatch.Length;
            if (i > titleMatch.Length)
            {
                int j = projectData.IndexOf('<', i + 1);
                title = projectData.Substring(i, j - i);
            }
            else
            {
                Console.WriteLine("Unable to find project title.");
                title = string.Empty;
            }
            return new Tuple<string, string>(version, title);
        }
    }
}
