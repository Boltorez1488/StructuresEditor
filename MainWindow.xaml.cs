using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class MainWindow : INotifyPropertyChanged {
        public ProjectBrowser Caller;
        public ObservableCollection<object> Items = new ObservableCollection<object>();
        public bool NeedSave = true;

        public string ProjectPath;
        public string AdditonalFile = "";
        public string EmptyFile = "empty.h";
        public string CompilerOutName = "all.h";
        public string CompilerFolder = "structs";
        public string GlobalNamespace;
        public string SerializatorPath = "serialized.xml";
        public bool PrintOffsets = true;

        public bool OnLoading = false;

        private readonly string _projectFolder;
        private string _curDir;
        public void BeginProjectDir() {
            if (string.IsNullOrEmpty(_projectFolder))
                return;
            _curDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(_projectFolder);
        }

        public void EndProjectDir() {
            if (string.IsNullOrEmpty(_curDir))
                return;
            Directory.SetCurrentDirectory(_curDir);
            _curDir = null;
        }

        private string _crPathCreator;
        public string PathCreator {
            get => _crPathCreator;
            set {
                _crPathCreator = value;
                OnPropertyChanged();
            }
        }

        private bool _is64Bit;
        public bool Is64Bit {
            get => _is64Bit;
            set {
                _is64Bit = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void Log(string text) {
            var dt = DateTime.Now;
            logger.AppendText($"[{dt:MM.dd.yy H:mm:ss}]: {text}\n");
            logger.ScrollToEnd();
        }

        private void LogClear_OnClick(object sender, RoutedEventArgs e) {
            logger.Clear();
        }

        public MainWindow(string projPath, ProjectBrowser caller) {
            Caller = caller;
            Caller.OpenedProject = this;
            InitializeComponent();
            DataContext = this;
            items.ItemsSource = Items;

            ProjectPath = projPath;
            _projectFolder = System.IO.Path.GetDirectoryName(projPath);
            Constants.MainWindow = this;
            ProjectLoader.Load(this, true);
            Load();

            Title = $"Structures Editor [by Boltorez1488] ({ProjectPath})";
            Log($"Start Project - {ProjectPath}");

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        private void OnChildDelete(object sender) {
            var index = Items.IndexOf(sender);
            Items.RemoveAt(index);
            if (index != 0 && index == Items.Count)
                index--;
            if (Items.Count == 0) {
                items.Focus();
            } else if (Items[index] is Namespace space) {
                space.scroll.Focus();
            } else if (Items[index] is Enum en) {
                en.scroll.Focus();
            }
        }

        public static void Sort<T>(ObservableCollection<T> collection, Comparison<T> comparison) {
            var sortableList = collection.ToList();
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++) {
                var item = sortableList[i];
                var index = collection.IndexOf(item);
                if (index != i)
                    collection.Move(index, i);
            }
        }

        public static int Comparer(object x, object y) {
            switch (x) {
                case Struct xSt when y is Struct ySt:
                    return String.Compare(xSt.MainName, ySt.MainName, StringComparison.Ordinal);
                case Struct xSt when y is Enum yEn:
                    return String.Compare(xSt.MainName, yEn.MainName, StringComparison.Ordinal);
                case Struct xSt when y is Namespace ySp:
                    return String.Compare(xSt.MainName, ySp.MainName, StringComparison.Ordinal);

                case Enum xEn when y is Struct ySt:
                    return String.Compare(xEn.MainName, ySt.MainName, StringComparison.Ordinal);
                case Enum xEn when y is Enum yEn:
                    return String.Compare(xEn.MainName, yEn.MainName, StringComparison.Ordinal);
                case Enum xEn when y is Namespace ySp:
                    return String.Compare(xEn.MainName, ySp.MainName, StringComparison.Ordinal);

                case Namespace xSp when y is Struct ySt:
                    return String.Compare(xSp.MainName, ySt.MainName, StringComparison.Ordinal);
                case Namespace xSp when y is Enum yEn:
                    return String.Compare(xSp.MainName, yEn.MainName, StringComparison.Ordinal);
                case Namespace xSp when y is Namespace ySp:
                    return String.Compare(xSp.MainName, ySp.MainName, StringComparison.Ordinal);
            }

            return 0x0;
        }

        public void Sort() {
            Sort(Items, Comparer);
        }

        private void OnChildMainNameChanged(object sender) {
            if (OnLoading)
                return;
            Sort();
        }

        public object GetItem(string name) {
            foreach (var obj in Items) {
                switch (obj) {
                    case Namespace sp:
                        if (sp.MainName == name)
                            return sp;
                        break;
                    case Struct st:
                        if (st.MainName == name)
                            return st;
                        break;
                    case Enum en:
                        if (en.MainName == name)
                            return en;
                        break;
                }
            }

            return null;
        }

        private void FocusObj(object obj) {
            switch (obj) {
                case Namespace sp:
                    sp.FocusName();
                    break;
                case Struct st:
                    st.FocusName();
                    break;
                case Enum en:
                    en.FocusName();
                    break;
            }
        }

        private Namespace CreateNamespace(Namespace parent = null, string name = null) {
            var space = name == null ? new Namespace(parent) : new Namespace(parent) {MainName = name};
            space.OnDelete += OnChildDelete;
            space.OnMainNameChanged += OnChildMainNameChanged;
            Items.Add(space);
            return space;
        }

        private Enum CreateEnum(Namespace parent = null, string name = null) {
            var child = name == null ? new Enum(parent) : new Enum(parent) { MainName = name };
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildMainNameChanged;
            Items.Add(child);
            return child;
        }

        private Struct CreateStruct(Namespace parent = null, string name = null) {
            var child = name == null ? new Struct(parent) : new Struct(parent) { MainName = name };
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildMainNameChanged;
            Items.Add(child);
            return child;
        }

        private void GoToPath() {
            if (string.IsNullOrEmpty(PathCreator))
                return;
            var split = PathCreator.Split('.', ':').Where(x => !String.IsNullOrEmpty(x)).ToArray();
            object found;
            if (split.Length == 1) {
                found = GetItem(split[0]);
                if (found != null) {
                    FocusObj(found);
                }
            }

            Namespace cur = null;
            foreach (var s in split.Take(split.Length - 1)) {
                if (cur == null) {
                    found = GetItem(s);
                    if (found != null && found is Namespace sp) {
                        cur = sp;
                        if (!cur.expander.IsExpanded)
                            cur.expander.IsExpanded = true;
                    } else
                        return;
                } else {
                    found = cur.GetItem(s);
                    if (found != null && found is Namespace sp) {
                        cur = sp;
                        if (!cur.expander.IsExpanded)
                            cur.expander.IsExpanded = true;
                    } else
                        return;
                }
            }

            if (cur == null)
                return;
            found = cur.GetItem(split.Last());
            if(found != null)
                FocusObj(found);
        }

        private object CreatePath(Type type) {
            var split = PathCreator.Split('.', ':').Where(x => !String.IsNullOrEmpty(x)).ToArray();
            object found;
            if (split.Length == 1) {
                found = GetItem(split[0]);
                if (found != null) {
                    FocusObj(found);
                    MessageBox.Show($"[{split[0]}] already exists", "Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                if(type == typeof(Namespace))
                    return CreateNamespace(null, split[0]);
                if (type == typeof(Struct))
                    return CreateStruct(null, split[0]);
                if (type == typeof(Enum))
                    return CreateEnum(null, split[0]);
                return null;
            }

            string path = "";
            Namespace cur = null;
            foreach (var s in split.Take(split.Length - 1)) {
                if (path == "")
                    path = s;
                else
                    path += $".{s}";
                if (cur == null) {
                    var obj = GetItem(s);
                    if (obj != null && obj is Namespace sp) {
                        cur = sp;
                        continue;
                    }
                    if (obj == null) {
                        cur = CreateNamespace(null, s);
                    } else {
                        MessageBox.Show($"[{path}] is not Namespace", "PathFinder Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                } else {
                    if (!cur.expander.IsExpanded)
                        cur.expander.IsExpanded = true;
                    var obj = cur.GetItem(s);
                    Namespace parent;
                    if (obj != null && obj is Namespace sp) {
                        parent = cur;
                        cur = sp;
                        continue;
                    }
                    if (obj == null) {
                        parent = cur;
                        cur = cur.AddNamespace();
                        cur.MainName = s;
                    } else {
                        MessageBox.Show($"[{path}] is not Namespace", "PathFinder Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (cur == null) return null;
            if (!cur.expander.IsExpanded)
                cur.expander.IsExpanded = true;

            var name = split.Last();
            found = cur.GetItem(name);
            if (found != null) {
                FocusObj(found);
                MessageBox.Show($"[{path}.{name}] already exists", "Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            if (type == typeof(Namespace)) {
                var child = cur.AddNamespace();
                child.MainName = name;
                return child;
            }

            if (type == typeof(Struct)) {
                var child = cur.AddStruct();
                child.MainName = name;
                return child;
            }

            if (type == typeof(Enum)) {
                var child = cur.AddEnum();
                child.MainName = name;
                return child;
            }
            return null;
        }

        public void AttachNamespace(Namespace child) {
            var space = new Namespace();
            space.OnDelete += OnChildDelete;
            space.MainName = child.MainName;
            Items.Add(space);
            foreach (var item in child.Items) {
                switch (item) {
                    case Namespace obj:
                        obj.ParentReplace(space);
                        break;
                    case Struct obj:
                        obj.ParentReplace(space);
                        break;
                    case Enum obj:
                        obj.ParentReplace(space);
                        break;
                }

                space.Items.Add(item);
            }
            if (space.Items.Count != 0) {
                space.dummy.Visibility = Visibility.Collapsed;
            }
        }

        public void AttachStruct(Struct child) {
            var st = new Struct();
            st.OnDelete += OnChildDelete;
            st.MainName = child.MainName;
            Items.Add(st);
            foreach (var item in child.Items) {
                switch (item) {
                    case StructStruct obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        obj.ParentReplace(st);
                        break;
                    case StructEnum obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        obj.ParentReplace(st);
                        break;
                    case StructVar obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        break;
                    case StructPtr obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        break;
                }
                st.Items.Add(item);
            }
            if (st.Items.Count != 0) {
                st.dummy.Visibility = Visibility.Collapsed;
            }
        }

        public void AttachStruct(StructStruct child) {
            var st = new Struct();
            st.OnDelete += OnChildDelete;
            st.MainName = child.MainName;
            Items.Add(st);
            foreach (var item in child.Items) {
                switch (item) {
                    case StructStruct obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        obj.ParentReplace(st);
                        break;
                    case StructEnum obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        obj.ParentReplace(st);
                        break;
                    case StructVar obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        break;
                    case StructPtr obj:
                        obj.OnDelete -= child.OnChildDelete;
                        obj.OnFullSizeChanged -= child.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= child.OnChildOffsetChanged;

                        obj.OnDelete += st.OnChildDelete;
                        obj.OnFullSizeChanged += st.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += st.OnChildOffsetChanged;
                        break;
                }
                st.Items.Add(item);
            }
            if (st.Items.Count != 0) {
                st.dummy.Visibility = Visibility.Collapsed;
            }
        }

        public void AttachEnum(Enum child) {
            var en = new Enum();
            en.OnDelete += OnChildDelete;
            en.MainName = child.MainName;
            Items.Add(en);
            foreach (var item in child.Items) {
                var field = en.AddField();
                field.MainValue = item.MainValue;
                field.Field = item.Field;
            }
        }

        public void AttachEnum(StructEnum child) {
            var en = new Enum();
            en.OnDelete += OnChildDelete;
            en.MainName = child.MainName;
            Items.Add(en);
            foreach (var item in child.Items) {
                var field = en.AddField();
                field.MainValue = item.MainValue;
                field.Field = item.Field;
            }
        }

        public Namespace AddNamespace() {
            if (String.IsNullOrEmpty(PathCreator)) {
                return CreateNamespace();
            }
            return CreatePath(typeof(Namespace)) as Namespace;
        }

        public Struct AddStruct() {
            if (String.IsNullOrEmpty(PathCreator)) {
                var st = new Struct();
                st.OnDelete += OnChildDelete;
                Items.Add(st);
                return st;
            }
            return CreatePath(typeof(Struct)) as Struct;
        }

        public Enum AddEnum() {
            if (String.IsNullOrEmpty(PathCreator)) {
                var en = new Enum();
                en.OnDelete += OnChildDelete;
                Items.Add(en);
                return en;
            }
            return CreatePath(typeof(Enum)) as Enum;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            items.Focus();
        }

        private void AddNBtn_OnClick(object sender, RoutedEventArgs e) {
            AddNamespace();
            Sort();
        }

        private void AddSBtn_OnClick(object sender, RoutedEventArgs e) {
            AddStruct();
            Sort();
        }

        private void AddEBtn_OnClick(object sender, RoutedEventArgs e) {
            AddEnum();
            Sort();
        }

        public void NamespaceAdder(Object sender, ExecutedRoutedEventArgs e) {
            AddNamespace();
            Sort();
        }

        private void StructAdder(object sender, ExecutedRoutedEventArgs e) {
            AddStruct();
            Sort();
        }

        private void EnumAdder(object sender, ExecutedRoutedEventArgs e) {
            AddEnum();
            Sort();
        }

        private void FocusPath(object sender, ExecutedRoutedEventArgs e) {
            pathBox.Focus();
            pathBox.SelectAll();
        }

        private void GoToPath(object sender, ExecutedRoutedEventArgs e) {
            GoToPath();
        }

        private void PathBox_OnKeyUp(object sender, KeyEventArgs e) {
            Utils.EnterText(sender);
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e) {
            PathCreator = "";
        }

        private void Save() {
            if (string.IsNullOrEmpty(SerializatorPath)) {
                Log("SerializatorPath is empty");
                return;
            }
            Log($"Serialize - {SerializatorPath}");
            BeginProjectDir();
            Serializer.Save(SerializatorPath);
            EndProjectDir();
        }

        private void Load() {
            if (string.IsNullOrEmpty(SerializatorPath)) {
                Log("SerializatorPath is empty");
                return;
            }
            Log($"Deserialize - {SerializatorPath}");
            OnLoading = true;
            BeginProjectDir();
            Deserializer.Load(SerializatorPath);
            EndProjectDir();
            OnLoading = false;
        }

        private void Save(object sender, ExecutedRoutedEventArgs e) {
            var focus = Keyboard.FocusedElement;
            if (focus is TextBox tb) {
                Utils.EnterText(tb);
            }
            Save();
        }

        private void Load(object sender, ExecutedRoutedEventArgs e) {
            Load();
        }

        private void Compile(object sender, ExecutedRoutedEventArgs e) {
            var folder = CompilerFolder;
            var name = CompilerOutName;
            Log(string.IsNullOrEmpty(folder) ? $"Start Compile - {name}" : $"Start Compile - {folder}/{name}");
            BeginProjectDir();
            EmptyCreator.Create(EmptyFile, CompilerFolder);
            Compiler.Compile(CompilerOutName, CompilerFolder, EmptyFile);
            EndProjectDir();
            Log("End Compile");
        }

        private void About_OnClick(object sender, RoutedEventArgs e) {
            var about = new AboutWindow();
            about.ShowDialog();
        }

        private void Properties_OnClick(object sender, RoutedEventArgs e) {
            var props = new PropertiesWindow(this);
            props.ShowDialog();
        }

        private bool _projectReturn;
        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            ProjectSaver.Save(this, true);

            if (NeedSave && !string.IsNullOrEmpty(SerializatorPath)) {
                BeginProjectDir();
                Serializer.Save(SerializatorPath);
                EndProjectDir();
            }
            
            Constants.MainWindow = null;
            if (!_projectReturn) {
                Caller.Close();
            } else {
                Caller.OpenedProject = null;
                Caller.Show();
            }
        }

        private void ExitBtn_OnClick(object sender, RoutedEventArgs e) {
            NeedSave = false;
            Close();
        }

        private void CloseProject(object sender, ExecutedRoutedEventArgs e) {
            _projectReturn = true;
            Close();
        }
    }
}
