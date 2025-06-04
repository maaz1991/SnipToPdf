using System;
using System.Threading;
using System.Windows.Forms;

namespace SnipToPdf
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // single-instance guard
            using var mtx = new Mutex(true, "SnipToPdfMutex", out bool first);
            if (!first)
            {
                MessageBox.Show("SnipToPdf is already running.");
                return;
            }

            // critical: per-monitor DPI so WinForms gives us real pixels
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
