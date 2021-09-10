using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace StructuresEditor {
    public partial class Enum : IEnum, INotifyPropertyChanged, IElement {
        public delegate void NameChangedEvent(Enum sender);

        public ObservableCollection<EnumField> Items = new ObservableCollection<EnumField>();
        private string _name;
        private string _parentPath = "";
        public object MainParent { get; set; }

        public Enum(object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = "Enum";
            MainParent = parent;
            ParentInit();

            addFBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
        }

        public Enum(string name, object parent = null) {
            InitializeComponent();
            DataContext = this;

            MainName = name;
            MainParent = parent;
            ParentInit();

            addFBtn.Foreground = new SolidColorBrush(Constants.EnumColor);
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

        public event NameChangedEvent OnMainNameChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public delegate void DeleteEvent(Enum sender);

        public event DeleteEvent OnDelete;

        public delegate void ParentPathChangedEvent(Enum sender);

        public event ParentPathChangedEvent OnParentPathChanged;

        public void OnChildUp(EnumField sender) {
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

        public void OnChildDown(EnumField sender) {
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

        public void OnChildDelete(EnumField sender) {
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

        public void OnChildValueChanged(EnumField sender) {
            if (Constants.MainWindow.OnLoading)
                return;
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
                if(index != i)
                    collection.Move(index, i);
            }
        }

        public void Sort() {
            //var list = Items.ToList();
            //list.Sort((x, y) => x.MainValue.CompareTo(y.MainValue));
            //Items.Clear();
            //foreach (var item in list) {
            //    Items.Add(item);
            //}
            Sort(Items, (x, y) => x.MainValue.CompareTo(y.MainValue));
        }

        public void FocusName() {
            nameBox.Focus();
            nameBox.SelectAll();
        }

        private void FocusName(object sender, ExecutedRoutedEventArgs e) {
            FocusName();
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

        private void OpenBtn_OnClick(object sender, RoutedEventArgs e) {
            var sub = new EnumWindow(this);
            sub.ShowDialog();
            items.Items.Refresh();
        }
    }
}