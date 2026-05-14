using System;
using System.Windows;

namespace CyberSecurityChatbot
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            Application app = new Application();
            app.Run(new MainWindow());
        }
    }
}