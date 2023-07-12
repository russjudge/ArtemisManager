using System;

namespace VersionBump
{
    class Program
    {
        const string versionMatch1 = "<FileVersion>";
        const string versionMatch2 = "<AssemblyVersion>";
        const string match3 = "</PropertyGroup>";
        static string NewFileVersion;
        static string NewAssemblyVersion;

        static string UpdateMatch(string projectData, string match, out string theNewVersion)
        {
            string retVal;
            theNewVersion = string.Empty;
            int i = projectData.IndexOf(match) + match.Length;
            if (i >= match.Length)
            {
                int j = projectData.IndexOf('<', i);
                string version = projectData.Substring(i, j - i);
                string[] parts = version.Split('.');
                if (parts.Length > 3)
                {
                    int k = int.Parse(parts[3]);
                    k++;
                    string newVersion = parts[0] + "." + parts[1] + "." + parts[2] + "." + k.ToString();
                    theNewVersion = newVersion;
                    retVal = projectData.Substring(0, i) + newVersion + projectData.Substring(j);
                }
                else if (parts.Length <= 3)
                {
                    string newVersion = parts[0] + "." + parts[1] + "." + parts[2] + ".1";
                    theNewVersion = newVersion;
                    retVal = projectData.Substring(0, i) + newVersion + projectData.Substring(j);
                }
                else
                {
                    retVal = InsertMissingVersionMatch(projectData, match);
                }
            }
            else
            {
                retVal = InsertMissingVersionMatch(projectData, match);
            }
            return retVal;
        }
        static string InsertMissingVersionMatch(string projectData, string match)
        {
            int i = projectData.IndexOf(match3, StringComparison.InvariantCultureIgnoreCase);
            string retVal = projectData.Substring(0, i) + "  " + match + "1.0.0.0</" + match.Substring(1) + "\r\n  " + projectData.Substring(i);
            return retVal;
        }
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string projectFile = args[0];
                string projectData;
                using (System.IO.StreamReader sr = new System.IO.StreamReader(projectFile))
                {
                    projectData = sr.ReadToEnd();
                }

                projectData = UpdateMatch(projectData, versionMatch1, out NewFileVersion);
                projectData = UpdateMatch(projectData, versionMatch2, out NewAssemblyVersion);

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(projectFile))
                {
                    sw.Write(projectData);
                }
                Console.WriteLine("****************************************************");
                Console.WriteLine("***   Project FileVersion update to    " + NewFileVersion + "   ***");
                Console.WriteLine("***   Project AsemblyVersion update to " + NewAssemblyVersion + "   ***");
                Console.WriteLine("****************************************************");

            }
            else
            {
                Console.WriteLine("Project file to update must be included on the command line.");
            }
        }
    }
}
