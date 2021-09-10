using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StructuresEditor {
    public partial class Namespace : INotifyPropertyChanged, IElement {
        public delegate void DeleteEvent(Namespace sender);

        public delegate void NameChangedEvent(Namespace sender);

        public delegate void ParentPathChangedEvent(Namespace sender);

        private string _name;

        private string _parentPath = "";

        public ObservableCollection<object> Items = new ObservableCollection<object>();

        public object MainParent { get; set; }

        public Namespace(Namespace parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = "Namespace";
            MainParent = parent;
            ParentInit();

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public Namespace(string name, Namespace parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = name;
            MainParent = parent;
            ParentInit();

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        private void ParentInit() {
            parentPath.Visibility = Visibility.Collapsed;
            if (MainParent == null)
                return;
            if (MainParent is Namespace space) {
                space.OnMainNameChanged += OnParentNameChanged;
                space.OnParentPathChanged += OnParentNameChanged;
            }
            OnParentNameChanged(MainParent);
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

        public string MainName {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged();
                OnMainNameChanged?.Invoke(this);
            }
        }

        public string ParentPath {
            get => _parentPath;
            set {
                _parentPath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event NameChangedEvent OnMainNameChanged;
        public event ParentPathChangedEvent OnParentPathChanged;
        public event DeleteEvent OnDelete;

        public void OnChildDelete(object sender) {
            var index = Items.IndexOf(sender);
            if (index >= 0) {
                switch (sender) {
                    case Namespace sp:
                        sp.OnDelete -= OnChildDelete;
                        sp.OnMainNameChanged -= OnChildNameChanged;
                        break;
                    case Struct st:
                        st.OnDelete -= OnChildDelete;
                        st.OnMainNameChanged -= OnChildNameChanged;
                        break;
                    case Enum en:
                        en.OnDelete -= OnChildDelete;
                        en.OnMainNameChanged -= OnChildNameChanged;
                        break;
                }
            }
            Items.RemoveAt(index);
            if (index != 0 && index == Items.Count)
                index--;
            if (Items.Count == 0) {
                dummy.Visibility = Visibility.Visible;
                scroll.Focus();
            } else if(Items[index] is Namespace space) {
                space.scroll.Focus();
            } else if (Items[index] is Enum en) {
                en.scroll.Focus();
            }
        }

        public void OnChildNameChanged(object sender) {
            if (Constants.MainWindow.OnLoading)
                return;
            Sort();
        }

        public void AttachNamespace(Namespace child) {
            var space = AddNamespace();
            space.MainName = child.MainName;
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
            var st = AddStruct();
            st.MainName = child.MainName;
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
            var st = AddStruct();
            st.MainName = child.MainName;
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
            var en = AddEnum();
            en.MainName = child.MainName;
            foreach (var item in child.Items) {
                var field = en.AddField();
                field.MainValue = item.MainValue;
                field.Field = item.Field;
            }
        }

        public void AttachEnum(StructEnum child) {
            var en = AddEnum();
            en.MainName = child.MainName;
            foreach (var item in child.Items) {
                var field = en.AddField();
                field.MainValue = item.MainValue;
                field.Field = item.Field;
            }
        }

        public Namespace AddNamespace() {
            if (!expander.IsExpanded)
                expander.IsExpanded = true;
            var child = new Namespace(this);
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            dummy.Visibility = Visibility.Collapsed;
            return child;
        }

        public Enum AddEnum() {
            if (!expander.IsExpanded)
                expander.IsExpanded = true;
            var child = new Enum(this);
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            dummy.Visibility = Visibility.Collapsed;
            return child;
        }

        public Struct AddStruct() {
            if (!expander.IsExpanded)
                expander.IsExpanded = true;
            var child = new Struct(this);
            child.OnDelete += OnChildDelete;
            child.OnMainNameChanged += OnChildNameChanged;
            Items.Add(child);
            dummy.Visibility = Visibility.Collapsed;
            return child;
        }

        private void Delete() {
            OnDelete?.Invoke(this);
        }

        private void addNBtn_Click(object sender, RoutedEventArgs e) {
            AddNamespace();
        }

        private void addSBtn_Click(object sender, RoutedEventArgs e) {
            AddStruct();
        }

        private void addEBtn_Click(object sender, RoutedEventArgs e) {
            AddEnum();
        }

        private void delBtn_Click(object sender, RoutedEventArgs e) {
            Delete();
        }

        private void OnParentNameChanged(object sender) {
            var names = new List<string>();
            var ucParent = MainParent;
            while (ucParent != null) {
                if (ucParent is Namespace space) {
                    names.Add(space.MainName);
                    ucParent = space.MainParent;
                } else {
                    break;
                }
            }

            if (names.Count != 0) {
                names.Reverse();
                var endPath = "";
                foreach (var n in names) endPath += n + ".";
                parentPath.Visibility = Visibility.Visible;
                ParentPath = endPath.Remove(endPath.Length - 1);
                OnParentPathChanged?.Invoke(this);
            }
        }

        public void ParentReplace(Namespace parent) {
            if (MainParent is Namespace old) {
                old.OnMainNameChanged -= OnParentNameChanged;
                old.OnParentPathChanged -= OnParentNameChanged;
            }

            MainParent = parent;
            parentPath.Visibility = Visibility.Collapsed;
            if (MainParent == null)
                return;
            parent.OnMainNameChanged += OnParentNameChanged;
            parent.OnParentPathChanged += OnParentNameChanged;
            OnParentNameChanged(MainParent);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            items.ItemsSource = Items;
            nameBox.Focus();
            nameBox.SelectAll();
        }

        private void nameBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            if (nameBox.SelectionLength != 0) {
                var start = nameBox.SelectionStart;
                var text = nameBox.Text.Remove(start, nameBox.SelectionLength).Insert(start, e.Text);
                var regex = new Regex("[A-z]{1}[A-z0-9]{0,}");
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            } else {
                var regex = new Regex("[A-z]{1}[A-z0-9]{0,}");
                var text = nameBox.Text.Insert(nameBox.CaretIndex, e.Text);
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            }
        }

        private void nameBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                Keyboard.ClearFocus();
                scroll.Focus();
            }
        }

        public void NamespaceAdder(object sender, ExecutedRoutedEventArgs e) {
            AddNamespace();
        }

        public void EnumAdder(object sender, ExecutedRoutedEventArgs e) {
            AddEnum();
        }

        private void StructAdder(object sender, ExecutedRoutedEventArgs e) {
            AddStruct();
        }

        public void DeleteCurrent(object sender, ExecutedRoutedEventArgs e) {
            Delete();
        }

        public void FocusName() {
            nameBox.Focus();
            nameBox.SelectAll();
        }

        private void FocusName(object sender, ExecutedRoutedEventArgs e) {
            FocusName();
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

        private void OpenBtn_OnClick(object sender, RoutedEventArgs e) {
            var sub = new SubWindow(this);
            sub.ShowDialog();
            //items.ItemsSource = Items;
            items.Items.Refresh();
        }

        private void MoveBtn_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ElementMover.MainDialog(this, MainParent);
            if (dlg.ShowDialog() == true) {
                if (dlg.Selected == null) {
                    OnDelete?.Invoke(this);
                    Constants.MainWindow.AttachNamespace(this);
                    return;
                }
                if (dlg.Selected is Namespace space) {
                    OnDelete?.Invoke(this);
                    space.AttachNamespace(this);
                }
            }
        }
    }
}