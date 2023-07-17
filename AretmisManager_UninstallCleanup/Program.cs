// See https://aka.ms/new-console-template for more information
DeleteFolder(ArtemisManagerAction.ModManager.DataFolder);
static void DeleteFolder(string target)
{
    if (Directory.Exists(target))
    {
        foreach (var dir in new DirectoryInfo(target).GetDirectories())
        {
            DeleteFolder(dir.FullName);
            dir.Delete();
        }
        foreach (var fle in new DirectoryInfo(target).GetFiles())
        {
            fle.Delete();
        }
    }
}