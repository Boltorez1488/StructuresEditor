using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace StructuresEditor {
    public partial class ProjectBrowser {
        public ObservableCollection<ProjectItem> Items = new ObservableCollection<ProjectItem>();
        public MainWindow OpenedProject;

        private void OnSelect(ProjectItem sender) {
            if (!File.Exists(sender.path.Text)) {
                sender.OnSelected -= OnSelect;
                Items.Remove(sender);
                return;
            }
            var win = new MainWindow(sender.path.Text, this);
            win.Show();
            Hide();
        }

        public ProjectBrowser(bool needOpen = true) {
            InitializeComponent();
            items.ItemsSource = Items;
            var open = Load();
            if (!needOpen) return;
            if (open != null && File.Exists(open))
                Open(open);
            else {
                Show();
            }
        }

        public void Open(string fpath) {
            AddProject(fpath);
            var win = new MainWindow(fpath, this);
            win.Show();
            Hide();
        }

        private void AddProject(string fpath) {
            var count = Items.Count(x => x.path.Text == fpath);
            if (count != 0) {
                var found = Items.First(x => x.path.Text == fpath);
                var index = Items.IndexOf(found);
                if (index != 0) Items.Move(index, 0);
            } else {
                var proj = new ProjectItem(fpath);
                proj.OnSelected += OnSelect;
                Items.Insert(0, proj);
            }
        }

        private void CreateProject(object sender, ExecutedRoutedEventArgs e) {
            var dlg = new SaveFileDialog { Filter = @"Structures Project|*.sproj" };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) Open(dlg.FileName);
        }

        private void OpenProject(object sender, ExecutedRoutedEventArgs e) {
            var dlg = new OpenFileDialog { Filter = @"Structures Project|*.sproj" };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) Open(dlg.FileName);
        }


        private void ProjectBrowser_OnClosing(object sender, CancelEventArgs e) {
            Save();
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private readonly string _exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        private string Load() {
            if (!File.Exists($"{_exePath}/{Constants.ProjectsFile}"))
                return null;
            var xDoc = XDocument.Load($"{_exePath}/{Constants.ProjectsFile}");
            var root = xDoc.Root;

            var projects = root?.Element("Projects");
            if(projects != null) {
                foreach (var el in projects.Elements()) {
                    if (el.Name.LocalName == "Item") {
                        AddProject(el.Value);
                    }
                }
            }

            var opened = root?.Element("OpenedProject");
            return opened?.Value;
        }

        private void Save() {
            var xDoc = new XDocument();
            var root = new XElement("StructureProjects");
            xDoc.Add(root);

            if(OpenedProject != null)
                root.Add(new XElement("OpenedProject", OpenedProject.ProjectPath));

            var projects = new XElement("Projects");
            root.Add(projects);
            foreach (var item in Items)
                projects.Add(new XElement("Item", item.path.Text));

            xDoc.Save($"{_exePath}/{Constants.ProjectsFile}");
        }
    }
}