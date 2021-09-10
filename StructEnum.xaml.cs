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
    // Special Enum for Struct
    public partial class StructEnum : IStructField, IEnum, INotifyPropertyChanged, IElement {
        public delegate void NameChangedEvent(StructEnum sender);

        public ObservableCollection<EnumField> Items = new ObservableCollection<EnumField>();
        private string _name;
        private string _parentPath = "";
        public object MainParent { get; set; }

        private string _var;
        public string Variable {
            get => _var;
            set {
                _var = value.Trim();
                Size = SizeCompiler.DefaultSize;
                bool isArray = SizeCompiler.IsArray(_var);
                ArraySize = isArray ? SizeCompiler.ArrayExtract(_var) : 0x1;
                if (isArray) {
                    _var = _var.Remove(_var.LastIndexOf("[", StringComparison.Ordinal)) + $"[0x{ArraySize:X}]";
                }
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

        public StructEnum(object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = "Enum";
            MainParent = parent;
            ParentInit();
            Variable = "enum";
            Size = SizeCompiler.DefaultSize;
            Offset = 0x0;

            addFBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public StructEnum(string name, object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = name;
            MainParent = parent;
            ParentInit();
            Variable = "enum";
            Size = SizeCompiler.DefaultSize;
            Offset = 0x0;

            addFBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
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
                    Variable = char.ToLower(_name[0]) + _name.Substring(1);
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

        public delegate void DeleteEvent(StructEnum sender);

        public event DeleteEvent OnDelete;

        public delegate void ParentPathChangedEvent(StructEnum sender);

        public event ParentPathChangedEvent OnParentPathChanged;

        public delegate void FullSizeChangedEvent(StructEnum sender);

        public event FullSizeChangedEvent OnFullSizeChanged;

        public delegate void OffsetChangedEvent(StructEnum sender);

        public event OffsetChangedEvent OnOffsetChanged;

        private void OnChildUp(EnumField sender) {
            var index = Items.IndexOf(sender);
            if (index != 0) {
                var prev = Items[index - 1];
                var field = prev.Field;
                prev.Field = sender.Field;
                sender.Field = field;
                var fbox = Items[index - 1].fieldBox;
                fbox.Focus();
                fbox.SelectAll();
                //Items.Move(index, index - 1);
            }
        }

        private void OnChildDown(EnumField sender) {
            var index = Items.IndexOf(sender);
            if (index != Items.Count - 1) {
                var prev = Items[index + 1];
                var field = prev.Field;
                prev.Field = sender.Field;
                sender.Field = field;
                var fbox = Items[index + 1].fieldBox;
                fbox.Focus();
                fbox.SelectAll();
                //Items.Move(index, index + 1);
            }
        }

        private void OnChildDelete(EnumField sender) {
            var index = Items.IndexOf(sender);
            Items.RemoveAt(index);
            if (index != 0 && index == Items.Count)
                index--;
            if (Items.Count == 0) {
                dummy.Visibility = Visibility.Visible;
                scroll.Focus();
            } else if (Items[index] is EnumField en) {
                en.fieldBox.Focus();
                en.fieldBox.SelectAll();
            }
        }

        private void OnChildValueChanged(EnumField sender) {
            Sort();
        }

        public EnumField AddField() {
            if (!expander.IsExpanded)
                expander.IsExpanded = true;
            if (Items.Count != 0) {
                var val = Items[Items.Count - 1]?.MainValue + 1;
                var child = new EnumField(val ?? Items.Count - 1) { MainParent = this };
                child.OnUp += OnChildUp;
                child.OnDown += OnChildDown;
                child.OnDelete += OnChildDelete;
                child.OnValueChanged += OnChildValueChanged;
                Items.Add(child);
                dummy.Visibility = Visibility.Collapsed;
                return child;
            } else {
                var child = new EnumField(0) { MainParent = this };
                child.OnUp += OnChildUp;
                child.OnDown += OnChildDown;
                child.OnDelete += OnChildDelete;
                child.OnValueChanged += OnChildValueChanged;
                Items.Add(child);
                dummy.Visibility = Visibility.Collapsed;
                return child;
            }
        }

        private void Delete() {
            OnDelete?.Invoke(this);
        }

        private void addFBtn_Click(object sender, RoutedEventArgs e) {
            AddField();
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
            if (e.Key == Key.Enter) {
                e.Handled = true;
                Keyboard.ClearFocus();
                scroll.Focus();
                if (!expander.IsExpanded)
                    expander.IsExpanded = true;
            }
        }

        public void FieldAdder(object sender, ExecutedRoutedEventArgs e) {
            AddField();
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

        public void Sort() {
            Sort(Items, (x, y) => x.MainValue.CompareTo(y.MainValue));
        }

        public void FocusName() {
            nameBox.Focus();
            nameBox.SelectAll();
        }

        private void FocusName(object sender, ExecutedRoutedEventArgs e) {
            FocusName();
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

        private void MoveBtn_OnClick(object sender, RoutedEventArgs e) {
            var dlg = new ElementMover.MainDialog(this, MainParent);
            if (dlg.ShowDialog() == true) {
                if (dlg.Selected == null) {
                    OnDelete?.Invoke(this);
                    Constants.MainWindow.AttachEnum(this);
                    return;
                }
                switch (dlg.Selected) {
                    case Namespace space:
                        OnDelete?.Invoke(this);
                        space.AttachEnum(this);
                        break;
                    case Struct st:
                        OnDelete?.Invoke(this);
                        st.AttachEnum(this);
                        break;
                }
            }
        }
    }
}
