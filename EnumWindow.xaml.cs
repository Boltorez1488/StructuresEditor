using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StructuresEditor {
    public partial class EnumWindow {
        public ObservableCollection<EnumField> Items;
        public Enum Root;

        public EnumWindow(Enum en) {
            InitializeComponent();

            Root = en;
            Root.items.ItemsSource = null;
            Items = Root.Items;
            items.ItemsSource = Items;

            Title = String.IsNullOrEmpty(en.ParentPath) ?
                $"Enum: {en.MainName}" : $"Enum: {en.ParentPath}.{en.MainName}";

            foreach (var item in Root.Items) {
                switch (item) {
                    case EnumField obj:
                        obj.OnUp -= Root.OnChildUp;
                        obj.OnDown -= Root.OnChildDown;
                        obj.OnDelete -= Root.OnChildDelete;
                        obj.OnValueChanged -= Root.OnChildValueChanged;

                        obj.OnUp += OnChildUp;
                        obj.OnDown += OnChildDown;
                        obj.OnDelete += OnChildDelete;
                        obj.OnValueChanged += OnChildValueChanged;
                        break;
                }
            }
        }

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
            }
        }

        private void OnChildDelete(EnumField sender) {
            var index = Items.IndexOf(sender);
            Items.RemoveAt(index);
            if (index != 0 && index == Items.Count)
                index--;
            if (Items.Count == 0) {
                return;
            }
            if (!(Items[index] is EnumField en))
                return;
            en.fieldBox.Focus();
            en.fieldBox.SelectAll();
        }

        private void OnChildValueChanged(EnumField sender) {
            Enum.Sort(Items, (x, y) => x.MainValue.CompareTo(y.MainValue));
        }

        public void AddField() {
            if (Items.Count != 0) {
                var val = Items[Items.Count - 1]?.MainValue + 1;
                var child = new EnumField(val ?? Items.Count - 1) { MainParent = this };
                child.OnUp += OnChildUp;
                child.OnDown += OnChildDown;
                child.OnDelete += OnChildDelete;
                child.OnValueChanged += OnChildValueChanged;
                Items.Add(child);
            } else {
                var child = new EnumField(0) { MainParent = this };
                child.OnUp += OnChildUp;
                child.OnDown += OnChildDown;
                child.OnDelete += OnChildDelete;
                child.OnValueChanged += OnChildValueChanged;
                Items.Add(child);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            items.Focus();
        }

        private void EnumWindow_OnClosed(object sender, EventArgs e) {
            foreach (var item in Items) {
                switch (item) {
                    case EnumField obj:
                        obj.OnUp -= OnChildUp;
                        obj.OnDown -= OnChildDown;
                        obj.OnDelete -= OnChildDelete;
                        obj.OnValueChanged -= OnChildValueChanged;

                        obj.OnUp += Root.OnChildUp;
                        obj.OnDown += Root.OnChildDown;
                        obj.OnDelete += Root.OnChildDelete;
                        obj.OnValueChanged += Root.OnChildValueChanged;
                        break;
                }
            }

            Root.dummy.Visibility = Items.Count != 0 ? Visibility.Collapsed : Visibility.Visible;

            items.ItemsSource = null;
            Root.items.ItemsSource = Root.Items;
        }

        private void FieldAdder(object sender, ExecutedRoutedEventArgs e) {
            AddField();
        }

        private void AddFBtn_OnClick(object sender, RoutedEventArgs e) {
            AddField();
        }
    }
}
