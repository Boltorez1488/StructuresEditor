using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StructuresEditor {
    public partial class EnumField : INotifyPropertyChanged {
        private string _field;

        public string Field {
            get => _field;
            set {
                _field = value;
                OnPropertyChanged();
            }
        }

        private int _value;
        public int MainValue {
            get => _value;
            set {
                _value = value;
                OnPropertyChanged();
                OnValueChanged?.Invoke(this);
            }
        }

        public object MainParent;
        
        public EnumField(int index) {
            InitializeComponent();
            DataContext = this;

            MainValue = index;
            Field = "Field";
            fieldBox.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public delegate void UpEvent(EnumField sender);
        public delegate void DownEvent(EnumField sender);
        public delegate void DeleteEvent(EnumField sender);
        public delegate void ValueChangedEvent(EnumField sender);

        public event UpEvent OnUp;
        public event DownEvent OnDown;
        public event DeleteEvent OnDelete;
        public event ValueChangedEvent OnValueChanged;

        private void delBtn_Click(object sender, RoutedEventArgs e) {
            Delete();
        }

        private void upBtn_Click(object sender, RoutedEventArgs e) {
            Up();
        }

        private void downBtn_Click(object sender, RoutedEventArgs e) {
            Down();
        }

        private void UpCommand(object sender, ExecutedRoutedEventArgs e) {
            if (Equals(Keyboard.FocusedElement, fieldBox)) {
                Utils.EnterText(fieldBox);
            }
            Up();
        }

        private void DownCommand(object sender, ExecutedRoutedEventArgs e) {
            if (Equals(Keyboard.FocusedElement, fieldBox)) {
                Utils.EnterText(fieldBox);
            }
            Down();
        }

        private void DeleteCommand(object sender, ExecutedRoutedEventArgs e) {
            Delete();
        }

        private void ValBox_OnMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    MainValue += 100;
                else if (Keyboard.IsKeyDown(Key.LeftShift))
                    MainValue += 10;
                else
                    MainValue++;
            } else {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    MainValue -= 100;
                else if (Keyboard.IsKeyDown(Key.LeftShift))
                    MainValue -= 10;
                else
                    MainValue--;
            }
        }

        private void fieldBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            var regex = new Regex("[A-z]{1}[A-z0-9]{0,}");
            if (fieldBox.SelectionLength != 0) {
                var start = fieldBox.SelectionStart;
                var text = fieldBox.Text.Remove(start, fieldBox.SelectionLength).Insert(start, e.Text);
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            } else {
                var text = fieldBox.Text.Insert(fieldBox.CaretIndex, e.Text);
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            }
        }

        private void ValBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e) {
            var regex = new Regex("[0-9]+");
            if (fieldBox.SelectionLength != 0) {
                var start = fieldBox.SelectionStart;
                var text = fieldBox.Text.Remove(start, fieldBox.SelectionLength).Insert(start, e.Text);
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            } else {
                var text = fieldBox.Text.Insert(fieldBox.CaretIndex, e.Text);
                var match = regex.Match(text);
                if (match.Index != 0)
                    e.Handled = true;
                else if (match.Length != text.Length)
                    e.Handled = true;
            }
        }

        #region Control

        private void Delete() {
            OnDelete?.Invoke(this);
        }

        private void Up() {
            OnUp?.Invoke(this);
        }

        private void Down() {
            OnDown?.Invoke(this);
        }

        #endregion

        private void EnumField_OnLoaded(object sender, RoutedEventArgs e) {
            fieldBox.Focus();
            fieldBox.SelectAll();
        }

        private void FieldBox_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
                if(MainParent != null) {
                    switch (MainParent) {
                        case Enum en:
                            en.scroll.Focus();
                            break;
                        case StructEnum sEn:
                            sEn.scroll.Focus();
                            break;
                    }
                }
                Utils.EnterText(sender);
                e.Handled = true;
            }
        }

        private void ValBox_OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
                if (MainParent != null) {
                    switch (MainParent) {
                        case Enum en:
                            en.scroll.Focus();
                            break;
                        case StructEnum sEn:
                            sEn.scroll.Focus();
                            break;
                    }
                }
                Utils.EnterText(sender);
                e.Handled = true;
            }
        }
    }
}