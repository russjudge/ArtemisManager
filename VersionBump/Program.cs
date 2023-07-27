using System;

namespace VersionBump
{
    class Program
    {
        private const string VersionMatch1 = "<FileVersion>";
        private const string VersionMatch2 = "<AssemblyVersion>";
        private const string Match3 = "</PropertyGroup>";
        private static string newFileVersion;
        private static string newAssemblyVersion;

        private static string UpdateMatch(string projectData, string match, out string theNewVersion)
        {
            string retVal;
            theNewVersion = string.Empty;
            int i = projectData.IndexOf(match) + match.Length;
            if (i >= 0)
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
                    retVal = projectData.Substring(0, i) + newVersion  + projectData.Substring(j);
                }
                else if (parts.Length <= 3)
                {
                    string newVersion = parts[0] + "." + parts[1] + "." + parts[2] + ".1";
                    theNewVersion = newVersion;
                    retVal = projectData.Substring(0, i) + newVersion  + projectData.Substring(j);
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
        private static string InsertMissingVersionMatch(string projectData, string match)
        {
            int i = projectData.IndexOf(Match3, StringComparison.InvariantCultureIgnoreCase);
            string retVal = projectData.Substring(0, i) + "  " + match + "1.0.0.0</" + match.Substring(1) + Environment.NewLine + "  " + projectData.Substring(i);
            return retVal;
        }
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string projectFile = args[0];
                string projectData;
                using (System.IO.StreamReader sr = new (projectFile))
                {
                    projectData = sr.ReadToEnd();
                }
               
                projectData = UpdateMatch(projectData, VersionMatch1, out newFileVersion);
                projectData = UpdateMatch(projectData, VersionMatch2, out newAssemblyVersion);

                using (System.IO.StreamWriter sw = new (projectFile))
                {
                    sw.Write(projectData);
                }
                Console.WriteLine("****************************************************");
                Console.WriteLine("***   Project FileVersion update to    " + newFileVersion + "   ***");
                Console.WriteLine("***   Project AsemblyVersion update to " + newAssemblyVersion + "   ***");
                Console.WriteLine("****************************************************");

            }
            else
            {
                Console.WriteLine("Project file to update must be included on the command line.");
            }
        }
    }
}
