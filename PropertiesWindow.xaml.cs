using System.Windows;

namespace StructuresEditor {
    public partial class PropertiesWindow {
        private readonly MainWindow _win;

        public PropertiesWindow(MainWindow win) {
            InitializeComponent();
            _win = win;

            addFile.Text = win.AdditonalFile;
            emptyFile.Text = win.EmptyFile;
            compilerOut.Text = win.CompilerOutName;
            compilerFolder.Text = win.CompilerFolder;
            serializerPath.Text = win.SerializatorPath;
            globalNamespace.Text = win.GlobalNamespace;
            compilerOffsets.IsChecked = win.PrintOffsets;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e) {
            _win.AdditonalFile = addFile.Text;
            _win.EmptyFile = emptyFile.Text;
            _win.CompilerOutName = compilerOut.Text;
            _win.CompilerFolder = compilerFolder.Text;
            _win.SerializatorPath = serializerPath.Text;
            _win.GlobalNamespace = globalNamespace.Text;
            _win.PrintOffsets = compilerOffsets.IsChecked != null && compilerOffsets.IsChecked.Value;
            Close();
        }
    }
}