using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StructuresEditor {
    public partial class ProjectItem {
        public delegate void SelectEvent(ProjectItem sender);

        public event SelectEvent OnSelected;

        public ProjectItem(string fpath) {
            InitializeComponent();

            fname.Text = System.IO.Path.GetFileNameWithoutExtension(fpath);
            path.Text = fpath;
        }

        private void Grid_OnMouseDown(object sender, MouseButtonEventArgs e) {
            OnSelected?.Invoke(this);
        }
    }
}
