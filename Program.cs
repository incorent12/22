using System;
using System.Windows.Forms;

namespace Gazeta
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            FormLogin loginForm = new FormLogin();
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                Application.Run(new FormMain());
            }
            else
            {
                Application.Exit();
            }
        }
    }
}