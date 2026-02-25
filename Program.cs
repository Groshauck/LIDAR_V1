using System;
using System.Windows.Forms;
using WinFormsApp1.Forms;

namespace WinFormsApp1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new ConfigurationForm()); // ✅ Démarrer sur ConfigurationForm
        }
    }
}