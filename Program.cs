namespace Gazeta
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain()); // Запускаем FormMain
        }
    }
}