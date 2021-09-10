using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class StructPtr : IStructField, INotifyPropertyChanged {
        public object Root; // Selected struct or enum
        public object PtrParent;

        private Color _prefixColor;
        public Color PrefixColor {
            get => _prefixColor;
            set {
                _prefixColor = value;
                OnPropertyChanged();
            }
        }

        private Color _ptrColor;
        public Color PtrColor {
            get => _ptrColor;
            set {
                _ptrColor = value;
                OnPropertyChanged();
            }
        }

        private string _ptrPath;
        public string PtrPath {
            get => _ptrPath;
            set {
                _ptrPath = value.Trim();
                if (Root is Enum) {
                    prefix.Text = "ENUM";
                    PrefixColor = PtrColor = Constants.EnumColor;
                } else if (Root is Struct) {
                    prefix.Text = "STRUCT";
                    PrefixColor = PtrColor = Constants.StructColor;
                }
                OnPropertyChanged();
            }
        }

        private string _var;
        public string Variable {
            get => _var;
            set {
                _var = value.Trim();
                Calc();
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
                if (_fullSize != value) {
                    _fullSize = value;
                    OnPropertyChanged();
                    toolSize.Content = $"Size: {_size}, Array: {ArraySize}";
                    OnFullSizeChanged?.Invoke(this);
                }
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Calc() {
            bool isPtr = SizeCompiler.IsPtr(_var);
            if (isPtr || Root is Enum) {
                Size = SizeCompiler.DefaultSize;
            } else {
                Size = 0x0;
                var observableCollection = (Root as Struct)?.Items;
                if (observableCollection != null && observableCollection.Count != 0) {
                    var obj = observableCollection.Last();
                    switch (obj) {
                        case StructVar v:
                            Size = v.Offset + v.FullSize;
                            break;
                        case StructPtr ptr:
                            Size = ptr.Offset + ptr.FullSize;
                            if (!SizeCompiler.IsPtr(ptr.Variable) && ptr.Root == PtrParent) {
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

        private void OnRootSortCalled(Struct sender) {
            if (Root == PtrParent)
                return;
            Calc();
        }

        private void OnRootItemAdded(Struct sender) {
            Calc();
        }

        private void OnRootItemRemoved(Struct sender) {
            if (Root == PtrParent)
                return;
            Calc();
        }

        private void SetPath(string parent, string name, string del = ".") {
            ptrPath.Document.Blocks.Clear();
            if (parent == null) {
                PtrPath = name;

                TextRange mainName = new TextRange(ptrPath.Document.ContentEnd, ptrPath.Document.ContentEnd) { Text = name };
                mainName.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Root is Struct ? Constants.StructColor : Constants.EnumColor));
            } else {
                PtrPath = parent + del + name;

                var parentPath = new TextRange(ptrPath.Document.ContentEnd, ptrPath.Document.ContentEnd) { Text = parent + del };
                parentPath.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightBlue);

                TextRange mainName = new TextRange(ptrPath.Document.ContentEnd, ptrPath.Document.ContentEnd) { Text = name };
                mainName.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Root is Struct ? Constants.StructColor : Constants.EnumColor));
            }
        }

        private void SetParentDelete(object obj) {
            Namespace p = null;
            switch (obj) {
                case Struct st:
                    p = st.MainParent as Namespace;
                    break;
                case Enum en:
                    p = en.MainParent as Namespace;
                    break;
            }

            while (p != null) {
                p.OnDelete += OnRootDelete;
                p = p.MainParent as Namespace;
            }
        }

        private void UnsetParentDelete(object obj) {
            Namespace p = null;
            switch (obj) {
                case Struct st:
                    p = st.MainParent as Namespace;
                    break;
                case Enum en:
                    p = en.MainParent as Namespace;
                    break;
            }

            while (p != null) {
                p.OnDelete -= OnRootDelete;
                p = p.MainParent as Namespace;
            }
        }

        private void SetRoot(object obj) {
            switch (obj) {
                case Struct st: {
                    Root = st;
                    st.OnMainNameChanged += OnAnyChanged;
                    st.OnParentPathChanged += OnAnyChanged;
                    st.OnDelete += OnRootDelete;
                    SetParentDelete(st);

                    st.OnSortCalled += OnRootSortCalled;
                    st.OnItemAdded += OnRootItemAdded;
                    st.OnItemRemoved += OnRootItemRemoved;
                    if (st.ParentPath is string parent) {
                        SetPath(st.MainParent != null && parent.Length != 0 ? parent : null, st.MainName);
                    }
                    if (!String.IsNullOrEmpty(st.MainName))
                        Variable = "*" + char.ToLower(st.MainName[0]) + st.MainName.Substring(1);
                    break;
                }
                case Enum en: {
                    Root = en;
                    en.OnMainNameChanged += OnAnyChanged;
                    en.OnParentPathChanged += OnAnyChanged;
                    en.OnDelete += OnRootDelete;
                    SetParentDelete(en);
                    if (en.ParentPath is string parent) {
                        SetPath(en.MainParent != null && parent.Length != 0 ? parent : null, en.MainName);
                    }
                    if (!String.IsNullOrEmpty(en.MainName))
                        Variable = char.ToLower(en.MainName[0]) + en.MainName.Substring(1);
                    break;
                }
            }
        }

        public StructPtr(Struct st) {
            InitializeComponent();
            DataContext = this;
            Offset = 0x0;
            Size = SizeCompiler.DefaultSize;
            ArraySize = 0x1;

            SetRoot(st);
        }

        public StructPtr(Enum en) {
            InitializeComponent();
            DataContext = this;
            Offset = 0x0;
            Size = SizeCompiler.DefaultSize;
            ArraySize = 0x1;

            SetRoot(en);
        }

        public delegate void DeleteEvent(StructPtr sender);

        public event DeleteEvent OnDelete;

        public delegate void FullSizeChangedEvent(StructPtr sender);

        public event FullSizeChangedEvent OnFullSizeChanged;

        public delegate void OffsetChangedEvent(StructPtr sender);

        public event OffsetChangedEvent OnOffsetChanged;

        public void OnAnyChanged(object sender) {
            switch (Root) {
                case Struct st when st.ParentPath is string stParent:
                    SetPath(stParent.Length != 0 ? stParent : null, st.MainName);
                    break;
                case Enum en when en.ParentPath is string enParent:
                    SetPath(enParent.Length != 0 ? enParent : null, en.MainName);
                    break;
            }
        }

        private void Replace() {
            var dlg = new PathSelector.MainDialog(PtrParent);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;
            switch (dlg.Selected) {
                case Struct st: { // Select Struct
                    switch (Root) {
                        case Struct old when !Equals(old, st):
                            old.OnMainNameChanged -= OnAnyChanged;
                            old.OnParentPathChanged -= OnAnyChanged;
                            old.OnDelete -= OnRootDelete;
                            UnsetParentDelete(old);

                            st.OnSortCalled -= OnRootSortCalled;
                            st.OnItemAdded -= OnRootItemAdded;
                            st.OnItemRemoved -= OnRootItemRemoved;
                            
                            SetRoot(st);
                            break;
                        case Enum olden:
                            olden.OnMainNameChanged -= OnAnyChanged;
                            olden.OnParentPathChanged -= OnAnyChanged;
                            olden.OnDelete -= OnRootDelete;
                            UnsetParentDelete(olden);

                            SetRoot(st);
                            break;
                    }

                    break;
                }
                case Enum en: { // Select Enum
                    switch (Root) {
                        case Enum old when !Equals(old, en):
                            old.OnMainNameChanged -= OnAnyChanged;
                            old.OnParentPathChanged -= OnAnyChanged;
                            old.OnDelete -= OnRootDelete;
                            UnsetParentDelete(old);

                            SetRoot(en);
                            break;
                        case Struct olden:
                            olden.OnMainNameChanged -= OnAnyChanged;
                            olden.OnParentPathChanged -= OnAnyChanged;
                            olden.OnDelete -= OnRootDelete;
                            UnsetParentDelete(olden);

                            SetRoot(en);
                            break;
                    }

                    break;
                }
            }
        }

        // If selected struct or enum call OnDelete
        private void OnRootDelete(object sender) {
            Delete();
        }

        private void Delete() {
            OnDelete?.Invoke(this);
        }

        private void delBtn_Click(object sender, RoutedEventArgs e) {
            Delete();
        }

        private void repBtn_Click(object sender, RoutedEventArgs e) {
            Replace();
        }

        private void Var_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
                Utils.EnterText(sender);
                e.Handled = true;
            }
        }

        private void Offset_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Offset += Utils.CalcWheel(e.Delta);
        }

        private void Size_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Size += Utils.CalcWheel(e.Delta);
        }

        private void StructPtr_OnLoaded(object sender, RoutedEventArgs e) {
            var.Focus();
            var.SelectAll();
        }

        private void DeleteCommand(object sender, ExecutedRoutedEventArgs e) {
            Delete();
        }
    }
}
