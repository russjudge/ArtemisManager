﻿using ArtemisManagerUI;
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
        static Mutex? Mutex;
        public static bool ProbablyUnattended { get; private set; }
        private void OnStartup(object sender, StartupEventArgs e)
        {
            bool createdNew;
            Mutex = new Mutex(true, mutextName, out createdNew);
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
                
                if (!Debugger.IsAttached)
                {
                    Task.Run(async () =>
                    {
                        var result = TakeAction.UpdateCheck(false);
                        if (result.Result.Item1)
                        {
                            if (await TakeAction.DoUpdate(true, result.Result.Item2))
                            {
                                TakeAction.MustExit = true;
                                Environment.Exit(0);
                            }
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
            //TODO: Handle error here.

            string workFile = Path.GetTempFileName() + ".txt";

            using (StreamWriter sw = new StreamWriter(workFile))
            {
                sw.WriteLine(e.Exception.ToString());
                sw.WriteLine();
                try
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (TakeAction.MainWindow != null)
                        {
                            foreach (var line in TakeAction.MainWindow.Status)
                            {
                                sw.WriteLine(line);
                            }
                        }
                    }));
                }
                catch
                {

                }
                MessageBox.Show("FATAL ERROR: " + e.Exception.Message +
                    "\r\n\r\nLoading debugging information.\r\nPlease cut and paste this information into the \"Contact Us\" form at:\r\nhttps://russjudge/contact\r\n\r\n" +
                    "We need to exit now....", "FATAL ERROR", MessageBoxButton.OK, MessageBoxImage.Error
                    );
                ProcessStartInfo startInfo = new(workFile);
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);

                startInfo = new("https://russjudge.com/contact");
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);

            }
        }
    }
}
