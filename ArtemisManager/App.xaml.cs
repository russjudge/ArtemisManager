using ArtemisManagerAction;
using ArtemisManagerUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ArtemisManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        const string mutextName = "D4133AFE-31E8-4351-85B0-7F27FB0AFD20"; //Is a generated GUID so that this will work regardless of where installed, even if installed in multiple locations.
                                                                          //"ArtemisManager";
#pragma warning disable IDE0052 // Remove unread private members
        static Mutex? Mutex;
#pragma warning restore IDE0052 // Remove unread private members
        public static bool ProbablyUnattended { get; private set; }
        private static void CreateFolders()
        {
            ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.ArtemisINIFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.ControlsINIFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.DMXCommandsFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ArtemisManager.EngineeringPresetsFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ModManager.ModArchiveFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ModItem.ModInstallFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ModItem.ActivatedFolder);
            ModManager.CreateFolder(ArtemisManagerAction.ModItem.MissionInstallFolder);
        }
        const string UpdateSoftwareSourceURL = "https://artemis.russjudge.com/software";
        const string UpdateVersionDataURL = UpdateSoftwareSourceURL + "/artemismanager.version";
        public static bool UpdateOnClose { get; private set; } = false;
        public static string UpdateInstallerPath { get; private set; } = string.Empty;
        private void OnStartup(object sender, StartupEventArgs e)
        {
            Mutex = new Mutex(true, mutextName, out bool createdNew);
            if (createdNew)
            {
                foreach (var arg in e.Args)
                {
                    if (arg.Equals("FROMSTARTUPFOLDER", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ProbablyUnattended = true;
                        break;
                    }
                }
                CreateFolders();
                if (!Debugger.IsAttached)
                {
                    var checker = new RussJudge.UpdateCheck.UpdateChecker(UpdateVersionDataURL);
                    checker.CheckForUpdate(true).ContinueWith((Result) =>
                    {
                        Console.WriteLine("Update check completed"); //, result = {0}", Result.Result.ToString());
                        switch (Result.Result)
                        {
                            case RussJudge.UpdateCheck.UpdateType.UpdateOnClose:
                                UpdateOnClose = true;
                                UpdateInstallerPath = checker.SetupFilePath;
                                TakeAction.MustExit = true;
                                break;
                            case RussJudge.UpdateCheck.UpdateType.UpdateNow:
                                UpdateInstallerPath = checker.SetupFilePath;

                                UpdateOnClose = true;
                                TakeAction.MustExit = true;
                                Shutdown(0);

                                break;
                        }

                    });

                }
            }
            else
            {

                TakeAction.MustExit = true;
                //MessageBox.Show("Exiting due to Mutex");
                Environment.Exit(0);
            }
        }

        private void OnError(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            try
            {
                string workFile = Path.GetTempFileName() + ".txt";

                using (StreamWriter sw = new(workFile))
                {
                    sw.WriteLine(e.Exception.ToString());
                    sw.WriteLine();

                }

                MessageBox.Show(string.Format("FATAL ERROR: --{0}--" +
                         "{1}{1}Loading debugging information.{1}Please cut and paste this information into the \"Contact Us\" form at:{1}https://russjudge/contact{1}{1}" +
                         "We need to exit now....", e.Exception.Message, Environment.NewLine), "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error
                         );
                ProcessStartInfo startInfo = new(workFile)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);

                startInfo = new("https://russjudge.com/contact")
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch
            {

            }
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (UpdateOnClose && !string.IsNullOrEmpty(UpdateInstallerPath))
            {
                if (RussJudge.UpdateCheck.UpdateChecker.IsDownloadingSetupFile && RussJudge.UpdateCheck.UpdateChecker.LockingObject != null)
                {
                    RussJudge.UpdateCheck.UpdateChecker.LockingObject.WaitOne();
                }
                if (File.Exists(UpdateInstallerPath))
                {
                    Process.Start(UpdateInstallerPath);
                }
                UpdateOnClose = false;  //To ensure it doesn't get called twice by mistake.
            }
        }
    }
}
