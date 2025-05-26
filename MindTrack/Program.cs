using System;
using System.Windows.Forms;

namespace MindTrack
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DatabaseHelper.EnsureDatabaseInitialized();
            Application.Run(new Form1());
        }
    }
}
