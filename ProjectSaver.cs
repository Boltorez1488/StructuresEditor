using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StructuresEditor {
    class ProjectSaver {
        public static void WinParams(XElement root, MainWindow window) {
            root.Add(new XElement("Top", Convert.ToInt32(window.Top)));
            root.Add(new XElement("Left", Convert.ToInt32(window.Left)));
            root.Add(new XElement("Width", Convert.ToInt32(window.Width)));
            root.Add(new XElement("Height", Convert.ToInt32(window.Height)));
            root.Add(new XElement("State", window.WindowState));
        }

        public static void Save(MainWindow window, bool winParams = false) {
            var xDoc = new XDocument();
            var root = new XElement("Pattern");
            xDoc.Add(root);

            if (winParams) {
                var win = new XElement("Window");
                WinParams(win, window);
                root.Add(win);
            }

            root.Add(new XElement("AdditonalFile", window.AdditonalFile));
            root.Add(new XElement("EmptyFile", window.EmptyFile));
            root.Add(new XElement("CompilerOutName", window.CompilerOutName));
            root.Add(new XElement("CompilerFolder", window.CompilerFolder));
            root.Add(new XElement("SerializatorPath", window.SerializatorPath));
            root.Add(new XElement("GlobalNamespace", window.GlobalNamespace));
            root.Add(new XElement("PrintOffsets", window.PrintOffsets));
            root.Add(new XElement("Is64Bit", window.Is64Bit));

            xDoc.Save(window.ProjectPath);
        }
    }
}
