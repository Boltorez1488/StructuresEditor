using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace StructuresEditor {
    class Constants {
        public static string ProjectsFile = "projects.sexml";
        public static MainWindow MainWindow = null;

        public static bool IsOffsetsShift() {
            return MainWindow.shiftOffsets.IsChecked != null && MainWindow.shiftOffsets.IsChecked.Value;
        }

        // Colors
        public static Color EnumColor = Color.FromRgb(184, 215, 163);
        public static Color StructColor = Color.FromRgb(78, 201, 176);
    }
}
