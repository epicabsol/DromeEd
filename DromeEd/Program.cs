using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DromeEd
{
    static class Program
    {
        public static ATD.VFS.Filesystem Filesystem = null;
        public static SplashWindow SplashWindow = null;
        public static MainWindow MainWindow = null;
        public static INIConfig Config = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Config = new INIConfig("DromeEd.ini");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SplashWindow = new SplashWindow();
            Application.Run(SplashWindow);
            if (SplashWindow.LoadSuccessful)
            {
                MainWindow = new MainWindow();
                Application.Run(MainWindow);
            }

            Config.Write("DromeEd.ini");
        }
    }
}
