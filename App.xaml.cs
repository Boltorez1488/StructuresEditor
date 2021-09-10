using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StructuresEditor {
    public partial class App {
        private void App_OnStartup(object sender, StartupEventArgs e) {
            if (e.Args.Length == 1 && File.Exists(e.Args[0])) {
                var wnd = new ProjectBrowser(false);
                wnd.Open(e.Args[0]);
            } else {
                var projectBrowser = new ProjectBrowser();
            }
        }
    }
}
