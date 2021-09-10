using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace StructuresEditor {
    static class ProjectLoader {
        private static void WinParams(XElement root, MainWindow window) {
            foreach (var el in root.Elements()) {
                try {
                    switch (el.Name.LocalName) {
                        case "Top":
                            window.Top = Convert.ToDouble(el.Value);
                            break;
                        case "Left":
                            window.Left = Convert.ToDouble(el.Value);
                            break;
                        case "Width":
                            window.Width = Convert.ToDouble(el.Value);
                            break;
                        case "Height":
                            window.Height = Convert.ToDouble(el.Value);
                            break;
                        case "State":
                            if (System.Enum.TryParse(el.Value, out WindowState ws))
                                window.WindowState = ws;
                            break;
                    }
                } catch {
                    // ignored
                }
            }
        }

        public static void Load(MainWindow window, bool winParams = false) {
            if (!File.Exists(window.ProjectPath))
                return;
            var xDoc = XDocument.Load(window.ProjectPath);
            var root = xDoc.Root;
            if (root == null)
                return;

            foreach (var el in root.Elements()) {
                switch (el.Name.LocalName) {
                    case "Window":
                        if(winParams)
                            WinParams(el, window);
                        break;
                    case "AdditonalFile":
                        window.AdditonalFile = el.Value;
                        break;
                    case "EmptyFile":
                        window.EmptyFile = el.Value;
                        break;
                    case "CompilerOutName":
                        window.CompilerOutName = el.Value;
                        break;
                    case "CompilerFolder":
                        window.CompilerFolder = el.Value;
                        break;
                    case "SerializatorPath":
                        window.SerializatorPath = el.Value;
                        break;
                    case "GlobalNamespace":
                        window.GlobalNamespace = el.Value;
                        break;
                    case "PrintOffsets":
                        window.PrintOffsets = Convert.ToBoolean(el.Value);
                        break;
                    case "Is64Bit":
                        window.Is64Bit = Convert.ToBoolean(el.Value);
                        break;
                }
            }
        }
    }
}
