using System;
using System.Windows.Forms;
namespace SimpleLauncher
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Launcher _Launcher = new Launcher();
            Application.Run(_Launcher);
       
        }

    }
}
