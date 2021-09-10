using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace StructuresEditor {
    public partial class SubWindow : INotifyPropertyChanged {
        public ObservableCollection<object> Items;// = new ObservableCollection<object>();
        public Namespace Root;

        private string _crPathCreator;
        public string PathCreator {
            get => _crPathCreator;
            set {
                _crPathCreator = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public SubWindow(Namespace space) {
            InitializeComponent();
            DataContext = this;

            Root = space;
            Root.items.ItemsSource = null;
            Items = Root.Items;
            items.ItemsSource = Items;

            Title = String.IsNullOrEmpty(space.ParentPath) ?
                $"Namespace: {space.MainName}" : $"Namespace: {space.ParentPath}.{space.MainName}";

            foreach (var item in Root.Items) {
                switch (item) {
                    case Namespace sp:
                        sp.OnDelete -= Root.OnChildDelete;
                        sp.OnMainNameChanged -= Root.OnChildNameChanged;

                        sp.OnDelete += OnChildDelete;
                        sp.OnMainNameChanged += OnChildNameChanged;
                        break;
                    case Struct st:
                        st.OnDelete -= Root.OnChildDelete;
                        st.OnMainNameChanged -= Root.OnChildNameChanged;

                        st.OnDelete += OnChildDelete;
                        st.OnMainNameChanged += OnChildNameChanged;
                        break;
                    case Enum en:
                        en.OnDelete -= Root.OnChildDelete;
                        en.OnMainNameChanged -= Root.OnChildNameChanged;

                        en.OnDelete += OnChildDelete;
                        en.OnMainNameChanged += OnChildNameChanged;
                        break;
                }
                //Items.Add(item);
            }
            //Root.Items.Clear();
        }

        public void OnChildDelete(object sender) {
            if (Items.Count == 0) {
                return;
            }
            var index = Items.IndexOf(sender);
            Items.RemoveAt(index);
            if (index != 0 && index == Items.Count)
                index--;
            if (Items.Count == 0) {
                //dummy.Visibility = Visibility.Visible;
                items.Focus();
            } else if (Items[index] is Namespace space) {
                space.scroll.Focus();
            } else if (Items[index] is Enum en) {
                en.scroll.Focus();
            }
        }

        public void OnChildNameChanged(object sender) {
            Sort();
        }

        private void Sort() {
            Namespace.Sort(Items, Namespace.Comparer);
        }

        private void AddNamespace() {
            if (String.IsNullOrEmpty(pathBox.Text)) {
                var child = new Namespace(Root);
                child.OnDelete += OnChildDelete;
                child.OnMainNameChanged += OnChildNameChanged;
                Items.Add(child);
                return;
            }
            CreatePath(typeof(Namespace));
        }

        private void AddStruct() {
            if (String.IsNullOrEmpty(pathBox.Text)) {
                var child = new Struct(Root);
                child.OnDelete += OnChildDelete;
                child.OnMainNameChanged += OnChildNameChanged;
                Items.Add(child);
                return;
            }
            CreatePath(typeof(Struct));
        }

        private void AddEnum() {
            if (String.IsNullOrEmpty(pathBox.Text)) {
                var child = new Enum(Root);
                child.OnDelete += OnChildDelete;
                child.OnMainNameChanged += OnChildNameChanged;
                Items.Add(child);
                return;
            }
            CreatePath(typeof(Enum));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            items.Focus();
        }

        private void AddNBtn_OnClick(object sender, RoutedEventArgs e) {
            AddNamespace();
        }

        private void AddSBtn_OnClick(object sender, RoutedEventArgs e) {
            AddStruct();
        }

        private void AddEBtn_OnClick(object sender, RoutedEventArgs e) {
            AddEnum();
        }

        public void NamespaceAdder(Object sender, ExecutedRoutedEventArgs e) {
            AddNamespace();
        }

        private void StructAdder(object sender, ExecutedRoutedEventArgs e) {
            AddStruct();
        }

        private void EnumAdder(object sender, ExecutedRoutedEventArgs e) {
            AddEnum();
        }

        private void SubWindow_OnClosed(object sender, EventArgs e) {
            foreach(var item in Items) {
                switch (item) {
                    case Namespace sp:
                        sp.OnDelete -= OnChildDelete;
                        sp.OnMainNameChanged -= OnChildNameChanged;

                        sp.OnDelete += Root.OnChildDelete;
                        sp.OnMainNameChanged += Root.OnChildNameChanged;
                        break;
                    case Struct st:
                        st.OnDelete -= OnChildDelete;
                        st.OnMainNameChanged -= OnChildNameChanged;

                        st.OnDelete += Root.OnChildDelete;
                        st.OnMainNameChanged += Root.OnChildNameChanged;
                        break;
                    case Enum en:
                        en.OnDelete -= OnChildDelete;
                        en.OnMainNameChanged -= OnChildNameChanged;

                        en.OnDelete += Root.OnChildDelete;
                        en.OnMainNameChanged += Root.OnChildNameChanged;
                        break;
                }
                //Root.Items.Add(item);
            }

            Root.dummy.Visibility = Items.Count != 0 ? Visibility.Collapsed : Visibility.Visible;

            items.ItemsSource = null;
            Root.items.ItemsSource = Root.Items;
            //Items.Clear();
        }

        private object GetItem(string name) {
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
            var child = name == null ? new Namespace(parent) : new Namespace(parent) { MainName = name };
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            return child;
        }

        private Enum CreateEnum(Namespace parent = null, string name = null) {
            var child = name == null ? new Enum(parent) : new Enum(parent) { MainName = name };
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            return child;
        }

        private Struct CreateStruct(Namespace parent = null, string name = null) {
            var child = name == null ? new Struct(parent) : new Struct(parent) { MainName = name };
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            return child;
        }

        private void GoToPath() {
            var split = pathBox.Text.Split('.', ':').Where(x => !String.IsNullOrEmpty(x)).ToArray();
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
            if (found != null)
                FocusObj(found);
        }

        private void CreatePath(Type type) {
            var split = pathBox.Text.Split('.', ':').Where(x => !String.IsNullOrEmpty(x)).ToArray();
            object found;
            if (split.Length == 1) {
                found = GetItem(split[0]);
                if (found != null) {
                    FocusObj(found);
                    MessageBox.Show($"[{split[0]}] already exists", "Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (type == typeof(Namespace))
                    CreateNamespace(Root, split[0]);
                else if (type == typeof(Struct))
                    CreateStruct(Root, split[0]);
                else if (type == typeof(Enum))
                    CreateEnum(Root, split[0]);
                return;
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
                        cur = CreateNamespace(Root, s);
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

            if (cur == null) return;
            if (!cur.expander.IsExpanded)
                cur.expander.IsExpanded = true;

            var name = split.Last();
            found = cur.GetItem(name);
            if (found != null) {
                FocusObj(found);
                MessageBox.Show($"[{path}.{name}] already exists", "Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (type == typeof(Namespace)) {
                cur.AddNamespace().MainName = name;
                return;
            }

            if (type == typeof(Struct)) {
                cur.AddStruct().MainName = name;
                return;
            }

            if (type == typeof(Enum)) {
                cur.AddEnum().MainName = name;
            }
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
    }
}