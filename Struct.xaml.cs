using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    public partial class Struct : IStruct, INotifyPropertyChanged, IElement {
        public ObservableCollection<object> Items = new ObservableCollection<object>();
        private string _name;
        private string _parentPath = "";
        public object MainParent { get; set; }

        public Struct(object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = "Struct";
            MainParent = parent;
            ParentInit();

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public Struct(string name, object parent = null) {
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
            if (MainParent is Namespace parent) {
                parent.OnMainNameChanged += OnParentNameChanged;
                parent.OnParentPathChanged += OnParentNameChanged;
            }
            OnParentNameChanged(MainParent);
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

        #region Events
        public delegate void NameChangedEvent(Struct sender);
        public event NameChangedEvent OnMainNameChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public delegate void DeleteEvent(Struct sender);

        public event DeleteEvent OnDelete;

        public delegate void ParentPathChangedEvent(Struct sender);

        public event ParentPathChangedEvent OnParentPathChanged;

        public delegate void SortCalledEvent(Struct sender);

        public event SortCalledEvent OnSortCalled;

        public delegate void ItemAddedEvent(Struct sender);

        public event ItemAddedEvent OnItemAdded;

        public delegate void ItemRemovedEvent(Struct sender);

        public event ItemRemovedEvent OnItemRemoved;
        #endregion

        public void ItemAdded() {
            OnItemAdded?.Invoke(this);
        }

        public void ItemRemoved() {
            OnItemRemoved?.Invoke(this);
        }

        public void OnChildDelete(object sender) {
            if (Items.Count == 0) {
                dummy.Visibility = Visibility.Visible;
                scroll.Focus();
                return;
            }
            var index = Items.IndexOf(sender);
            if (index < 0)
                return;
            Items.RemoveAt(index);
            ItemRemoved();
            if (Items.Count == 0) {
                dummy.Visibility = Visibility.Visible;
                scroll.Focus();
                return;
            }
            if (index > 0 && index == Items.Count)
                index--;
            switch (Items[index]) {
                case StructPtr ptr:
                    ptr.var.Focus();
                    ptr.var.SelectAll();
                    break;
                case StructVar v:
                    v.var.Focus();
                    v.var.SelectAll();
                    break;
                case StructEnum en:
                    en.var.Focus();
                    en.var.SelectAll();
                    break;
                case StructStruct ss:
                    ss.var.Focus();
                    ss.var.SelectAll();
                    break;
            }
        }

        private bool _onCalc;

        public void OnChildOffsetChanged(object sender) {
            if (Constants.MainWindow.OnLoading || _onCalc)
                return;

            if (Constants.IsOffsetsShift()) {
                var index = Items.IndexOf(sender);
                var sub = 0;
                if (sender is IStructField old)
                    sub = old.Offset - old.OldOffset;
                for (var i = index + 1; i < Items.Count; i++) {
                    if (Items[i] is IStructField item) item.Offset += sub;
                }
            }

            Sort();
        }

        public void OnChildFullSizeChanged(object sender) {
            if (Constants.MainWindow.OnLoading || _onCalc)
                return;
            Sort();
        }

        public object ExtractFocused() {
            var focus = Keyboard.FocusedElement;
            if (focus == null)
                return null;
            if (focus is Control ctrl) {
                var found = Items.FirstOrDefault(x => x == ctrl.DataContext);
                if (found == null)
                    return null;
                return found;
            }
            return null;
        }

        public void AddItem(object vp) {
            _onCalc = true;
            if (vp is IStructField var) {
                if (Items.Count == 0) {
                    var.Offset = 0x0;
                    Items.Add(var);
                } else if (ExtractFocused() is IStructField focus) {
                    var.Offset = focus.Offset + focus.FullSize;
                    Items.Insert(Items.IndexOf(focus) + 1, var);
                } else if (Items.Last() is IStructField item) {
                    var.Offset = item.Offset + item.FullSize;
                    Items.Add(var);
                }
            }
            _onCalc = false;
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
            en.Variable = child.Variable;
            en.Size = child.Size;
            en.ArraySize = child.ArraySize;
            foreach (var item in child.Items) {
                var field = en.AddField();
                field.MainValue = item.MainValue;
                field.Field = item.Field;
            }
        }

        public StructVar AddVar() {
            var var = new StructVar();
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(var);
            dummy.Visibility = Visibility.Collapsed;
            ItemAdded();
            return var;
        }

        public StructPtr AddPtr(object obj) {
            switch (obj) {
                case Struct st: {
                    var ptr = new StructPtr(st) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    AddItem(ptr);
                    dummy.Visibility = Visibility.Collapsed;
                    return ptr;
                }
                case Enum en: {
                    var ptr = new StructPtr(en) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    AddItem(ptr);
                    dummy.Visibility = Visibility.Collapsed;
                    return ptr;
                }
            }

            return null;
        }

        private void AddPtr() {
            var dlg = new PathSelector.MainDialog(this);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;
            switch (dlg.Selected) {
                case Struct st: {
                    var ptr = new StructPtr(st) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    AddItem(ptr);
                    dummy.Visibility = Visibility.Collapsed;
                    break;
                }
                case Enum en: {
                    var ptr = new StructPtr(en) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    AddItem(ptr);
                    dummy.Visibility = Visibility.Collapsed;
                    break;
                }
            }
            OnItemAdded?.Invoke(this);
        }

        public StructEnum AddEnum() {
            var var = new StructEnum(this);
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(var);
            dummy.Visibility = Visibility.Collapsed;
            OnItemAdded?.Invoke(this);
            return var;
        }

        public StructStruct AddStruct() {
            var var = new StructStruct(this);
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(var);
            dummy.Visibility = Visibility.Collapsed;
            OnItemAdded?.Invoke(this);
            return var;
        }

        private void Delete() {
            OnDelete?.Invoke(this);
        }

        private void addVBtn_Click(object sender, RoutedEventArgs e) {
            AddVar();
        }

        private void addPBtn_Click(object sender, RoutedEventArgs e) {
            AddPtr();
        }

        private void delBtn_Click(object sender, RoutedEventArgs e) {
            Delete();
        }

        public void OnParentNameChanged(object sender) {
            var names = new List<string>();
            var parent = MainParent;
            while (parent != null) {
                if (parent is Namespace p) {
                    names.Add(p.MainName);
                    parent = p.MainParent;
                }
            }
            if (names.Count != 0) {
                names.Reverse();
                var endPath = "";
                foreach (var n in names) {
                    endPath += n + ".";
                }
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
                Keyboard.ClearFocus();
                var tBox = sender as TextBox;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                binding?.UpdateSource();
                e.Handled = true;
                scroll.Focus();
                if (!expander.IsExpanded)
                    expander.IsExpanded = true;
            }
        }

        public void VarAdder(object sender, ExecutedRoutedEventArgs e) {
            AddVar();
        }

        public void DeleteCurrent(object sender, ExecutedRoutedEventArgs e) {
            Delete();
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
            //switch (x) {
            //    case StructPtr ptr when y is StructPtr ptr2:
            //        return (ptr.Offset + ptr.FullSize).CompareTo(ptr2.Offset);
            //    case StructVar var when y is StructVar var2:
            //        return (var.Offset + var.FullSize).CompareTo(var2.Offset);
            //    case StructPtr xptr when y is StructVar yvar:
            //        return (xptr.Offset + xptr.FullSize).CompareTo(yvar.Offset);
            //    case StructVar xvar when y is StructPtr yptr:
            //        return (xvar.Offset + xvar.FullSize).CompareTo(yptr.Offset);
            //}
            if (x is IStructField xF && y is IStructField yF) {
                return xF.Offset.CompareTo(yF.Offset);
            }
            return 0x0;
        }

        public void Sort() {
            Sort(Items, Comparer);
            OnSortCalled?.Invoke(this);
        }

        public void FocusName() {
            nameBox.Focus();
            nameBox.SelectAll();
        }

        private void FocusName(object sender, ExecutedRoutedEventArgs e) {
            FocusName();
        }

        private void addEBtn_Click(object sender, RoutedEventArgs e) {
            AddEnum();
        }

        private void PtrAdder(object sender, ExecutedRoutedEventArgs e) {
            AddPtr();
        }

        private void EnumAdder(object sender, ExecutedRoutedEventArgs e) {
            AddEnum();
        }

        private void addSBtn_Click(object sender, RoutedEventArgs e) {
            AddStruct();
        }

        private void StructAdder(object sender, ExecutedRoutedEventArgs e) {
            AddStruct();
        }

        private void MoveBtn_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ElementMover.MainDialog(this, MainParent);
            if (dlg.ShowDialog() == true) {
                if (dlg.Selected == null) {
                    OnDelete?.Invoke(this);
                    Constants.MainWindow.AttachStruct(this);
                    return;
                }
                switch (dlg.Selected) {
                    case Namespace space:
                        OnDelete?.Invoke(this);
                        space.AttachStruct(this);
                        break;
                    case Struct st:
                        OnDelete?.Invoke(this);
                        st.AttachStruct(this);
                        break;
                }
            }
        }

        private void OpenBtn_OnClick(object sender, RoutedEventArgs e) {
            var sub = new StructWindow(this);
            sub.ShowDialog();
            items.Items.Refresh();
        }
    }
}
