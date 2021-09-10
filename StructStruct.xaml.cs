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

namespace StructuresEditor
{
    public partial class StructStruct : IStructField, IStruct, INotifyPropertyChanged, IElement {
        public delegate void NameChangedEvent(StructStruct sender);

        public ObservableCollection<object> Items = new ObservableCollection<object>();
        private string _name;
        private string _parentPath = "";
        public object MainParent { get; set; }

        private string _var;
        public string Variable {
            get => _var;
            set {
                _var = value.Trim();
                //Size = 0x4;
                //bool isArray = SizeCompiler.IsArray(_var);
                //ArraySize = isArray ? SizeCompiler.ArrayExtract(_var) : 0x1;
                //if (isArray) {
                //    _var = _var.Remove(_var.LastIndexOf("[", StringComparison.Ordinal)) + $"[0x{ArraySize:X}]";
                //}
                VarCalc();
                OnPropertyChanged();
            }
        }

        private int _arraySize; // Size of array
        public int ArraySize {
            get => _arraySize;
            set {
                _arraySize = value < 1 ? 1 : value;
                OnPropertyChanged();
                FullSize = ArraySize * Size;
            }
        }
        private int _size; // Size of type
        public int Size {
            get => _size;
            set {
                _size = value < 1 ? 1 : value;
                OnPropertyChanged();
                FullSize = ArraySize * Size;
            }
        }
        private int _fullSize; // ArraySize * Size
        public int FullSize {
            get => _fullSize;
            set {
                if (_fullSize == value) return;
                _fullSize = value;
                OnPropertyChanged();
                toolSize.Content = $"Size: {_size}, Array: {ArraySize}";
                OnFullSizeChanged?.Invoke(this);
            }
        }

        public int OldOffset { get; set; }
        private int _offset;
        public int Offset {
            get => _offset;
            set {
                if (_offset == value) return;
                OldOffset = _offset;
                _offset = value < 0 ? 0 : value;
                OnPropertyChanged();
                toolOffset.Content = "Offset: " + _offset;
                OnOffsetChanged?.Invoke(this);
            }
        }

        private void VarCalc() {
            bool isPtr = SizeCompiler.IsPtr(_var);
            if (isPtr) {
                Size = SizeCompiler.DefaultSize;
            } else {
                Size = 0x0;
                if (Items != null && Items.Count != 0) {
                    var obj = Items.Last();
                    switch (obj) {
                        case StructVar v:
                            Size = v.Offset + v.FullSize;
                            break;
                        case StructPtr ptr:
                            Size = ptr.Offset + ptr.FullSize;
                            if (!SizeCompiler.IsPtr(ptr.Variable) && ptr.Root == MainParent) {
                                Size += ptr.FullSize;
                            }
                            break;
                    }
                }
            }
            if (!isPtr) {
                bool isArray = SizeCompiler.IsArray(_var);
                ArraySize = isArray ? SizeCompiler.ArrayExtract(_var) : 0x1;
                if (isArray) {
                    _var = _var.Remove(_var.LastIndexOf("[", StringComparison.Ordinal)) + $"[0x{ArraySize:X}]";
                }
            } else {
                ArraySize = 0x1;
            }
        }

        public StructStruct(object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = "Struct";
            MainParent = parent;
            ParentInit();
            Size = SizeCompiler.DefaultSize;
            Offset = 0x0;

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public StructStruct(string name, object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = name;
            MainParent = parent;
            ParentInit();
            Size = SizeCompiler.DefaultSize;
            Offset = 0x0;

            addSBtn.Foreground = new SolidColorBrush(Constants.StructColor);
            addEBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        private void ParentInit() {
            parentPath.Visibility = Visibility.Collapsed;
            if (MainParent == null)
                return;
            switch (MainParent) {
                case Namespace parent:
                    parent.OnMainNameChanged += OnParentNameChanged;
                    parent.OnParentPathChanged += OnParentNameChanged;
                    break;
                case Struct parent:
                    parent.OnMainNameChanged += OnParentNameChanged;
                    parent.OnParentPathChanged += OnParentNameChanged;
                    break;
                case StructStruct parent:
                    parent.OnMainNameChanged += OnParentNameChanged;
                    parent.OnParentPathChanged += OnParentNameChanged;
                    break;
            }
            OnParentNameChanged(MainParent);
        }

        public string MainName {
            get => _name;
            set {
                _name = value;
                OnPropertyChanged();
                OnMainNameChanged?.Invoke(this);
                if (!String.IsNullOrEmpty(_name))
                    Variable = "*" + char.ToLower(_name[0]) + _name.Substring(1);
            }
        }

        public string ParentPath {
            get => _parentPath;
            set {
                _parentPath = value;
                OnPropertyChanged();
            }
        }

        public event NameChangedEvent OnMainNameChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // Original Events
        public delegate void DeleteEvent(StructStruct sender);

        public event DeleteEvent OnDelete;

        public delegate void ParentPathChangedEvent(StructStruct sender);

        public event ParentPathChangedEvent OnParentPathChanged;

        public delegate void SortCalledEvent(StructStruct sender);

        public event SortCalledEvent OnSortCalled;

        public delegate void ItemAddedEvent(StructStruct sender);

        public event ItemAddedEvent OnItemAdded;

        public delegate void ItemRemovedEvent(StructStruct sender);

        public event ItemRemovedEvent OnItemRemoved;

        public delegate void FullSizeChangedEvent(StructStruct sender);

        // Variable Events
        public event FullSizeChangedEvent OnFullSizeChanged;

        public delegate void OffsetChangedEvent(StructStruct sender);

        public event OffsetChangedEvent OnOffsetChanged;

        public void ItemAdded() {
            VarCalc();
            OnItemAdded?.Invoke(this);
        }

        public void ItemRemoved() {
            VarCalc();
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
            VarCalc();
        }

        public void OnChildFullSizeChanged(object sender) {
            if (Constants.MainWindow.OnLoading || _onCalc)
                return;
            Sort();
            VarCalc();
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

        public StructVar AddVar() {
            var sv = new StructVar();
            sv.OnDelete += OnChildDelete;
            sv.OnFullSizeChanged += OnChildFullSizeChanged;
            sv.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(sv);
            dummy.Visibility = Visibility.Collapsed;
            ItemAdded();
            return sv;
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
            ItemAdded();
        }

        public StructEnum AddEnum() {
            var se = new StructEnum(this);
            se.OnDelete += OnChildDelete;
            se.OnFullSizeChanged += OnChildFullSizeChanged;
            se.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(se);
            dummy.Visibility = Visibility.Collapsed;
            ItemAdded();
            return se;
        }

        public StructStruct AddStruct() {
            var ss = new StructStruct(this);
            ss.OnDelete += OnChildDelete;
            ss.OnFullSizeChanged += OnChildFullSizeChanged;
            ss.OnOffsetChanged += OnChildOffsetChanged;
            AddItem(ss);
            dummy.Visibility = Visibility.Collapsed;
            ItemAdded();
            return ss;
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

        private void OnParentNameChanged(object sender) {
            var names = new List<string>();
            var parent = MainParent;
            while (parent != null) {
                switch (parent) {
                    case Namespace p:
                        names.Add(p.MainName);
                        parent = p.MainParent;
                        break;
                    case Struct p:
                        names.Add(p.MainName);
                        parent = p.MainParent;
                        break;
                    case StructStruct p:
                        names.Add(p.MainName);
                        parent = p.MainParent;
                        break;
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

        public void ParentReplace(Struct parent) {
            switch (MainParent) {
                case Struct old:
                    old.OnMainNameChanged -= OnParentNameChanged;
                    old.OnParentPathChanged -= OnParentNameChanged;
                    break;
                case StructStruct old:
                    old.OnMainNameChanged -= OnParentNameChanged;
                    old.OnParentPathChanged -= OnParentNameChanged;
                    break;
            }

            MainParent = parent;
            parentPath.Visibility = Visibility.Collapsed;
            if (MainParent == null)
                return;
            parent.OnMainNameChanged += OnParentNameChanged;
            parent.OnParentPathChanged += OnParentNameChanged;
            OnParentNameChanged(MainParent);
        }

        public void ParentReplace(StructStruct parent) {
            switch (MainParent) {
                case Struct old:
                    old.OnMainNameChanged -= OnParentNameChanged;
                    old.OnParentPathChanged -= OnParentNameChanged;
                    break;
                case StructStruct old:
                    old.OnMainNameChanged -= OnParentNameChanged;
                    old.OnParentPathChanged -= OnParentNameChanged;
                    break;
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
            if (e.Key != Key.Enter) return;
            Keyboard.ClearFocus();
            var tBox = sender as TextBox;
            DependencyProperty prop = TextBox.TextProperty;

            BindingExpression binding = BindingOperations.GetBindingExpression(tBox ?? throw new InvalidOperationException(), prop);
            binding?.UpdateSource();
            e.Handled = true;
            scroll.Focus();
            if (!expander.IsExpanded)
                expander.IsExpanded = true;
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

        private static int Comparer(object x, object y) {
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

        private void Offset_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Offset += Utils.CalcWheel(e.Delta);
        }

        private void Size_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Size += Utils.CalcWheel(e.Delta);
        }

        private void Var_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
                Utils.EnterText(sender);
                e.Handled = true;
            }
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
    }
}
