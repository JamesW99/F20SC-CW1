using System;
using Gtk;
using System.Collections.Generic;

namespace MyBrowser
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            
            Application.Init();

            var app = new Application("org.MyBrowser.MyBrowser", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
            
        }

        
    }
}
