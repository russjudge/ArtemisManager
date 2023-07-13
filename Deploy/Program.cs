// See https://aka.ms/new-console-template for more information
using Deploy;
//var args = Environment.GetCommandLineArgs();
try
{
    if (args.Length > 3)
    {
        var projectFile = args[0];
        var data = VersionDataFileBuilder.GetVersion(projectFile);
        VersionDataFileBuilder.BuildDataFile(data.Item1, data.Item2, args[1], args[2], args[3]);
        Console.WriteLine("Deploy:");
        Console.WriteLine("*** File {0} deployed to {1}", args[1], args[2]);
    }
    else
    {
        Console.WriteLine("Deploy:");
        Console.WriteLine("*** ERROR: Improper arguments.  Should be: \"$(ProjectPath)\" \"<SetupFileNameFullPath>\" \"$(OutDir)\" \"<targetPath>\"");
        Environment.Exit(1);
    }
}
catch (Exception ex)
{
    Console.WriteLine("Deploy:");
    Console.WriteLine(ex.ToString());
    Environment.Exit(1);
}
