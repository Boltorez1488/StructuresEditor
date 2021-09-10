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
    public partial class StructVar : IStructField, INotifyPropertyChanged {
        private string _var;
        public string Variable {
            get => _var;
            set {
                _var = value.Trim();
                Size = SizeCompiler.GetTypeSize(_var);
                if (!SizeCompiler.IsPtr(_var)) {
                    bool isArray = SizeCompiler.IsArray(_var);
                    ArraySize = isArray ? SizeCompiler.ArrayExtract(_var) : 0x1;
                    if (isArray) {
                        _var = _var.Remove(_var.LastIndexOf("[", StringComparison.Ordinal)) + $"[0x{ArraySize:X}]";
                    }
                } else {
                    ArraySize = 0x1;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public StructVar() {
            InitializeComponent();
            DataContext = this;

            Variable = "auto var";
            Size = SizeCompiler.DefaultSize;
            Offset = 0x0;
        }

        public delegate void DeleteEvent(StructVar sender);

        public event DeleteEvent OnDelete;

        public delegate void FullSizeChangedEvent(StructVar sender);

        public event FullSizeChangedEvent OnFullSizeChanged;

        public delegate void OffsetChangedEvent(StructVar sender);

        public event OffsetChangedEvent OnOffsetChanged;

        private void delBtn_Click(object sender, RoutedEventArgs e) {
            OnDelete?.Invoke(this);
        }

        private void Var_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
                var tBox = sender as TextBox;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                binding?.UpdateSource();
                e.Handled = true;
            }
        }

        private void Offset_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Offset += Utils.CalcWheel(e.Delta);
        }

        private void Size_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            Size += Utils.CalcWheel(e.Delta);
        }

        private void StructVar_OnLoaded(object sender, RoutedEventArgs e) {
            var.Focus();
            var.SelectAll();
        }

        private void DeleteCommand(object sender, ExecutedRoutedEventArgs e) {
            OnDelete?.Invoke(this);
        }
    }
}
