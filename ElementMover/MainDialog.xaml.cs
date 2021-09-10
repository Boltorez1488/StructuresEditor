using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using StructuresEditor.PathSelector;

namespace StructuresEditor.ElementMover {
    public partial class MainDialog {
        public ObservableCollection<Item> Items = new ObservableCollection<Item>();
        public ObservableCollection<Item> SearchItems = new ObservableCollection<Item>();
        public object Selected;
        public object Caller;
        public object CallerParent;

        public MainDialog(object caller, object parent) {
            InitializeComponent();
            DataContext = this;
            Caller = caller;
            CallerParent = parent;
            items.ItemsSource = Items;

            // MainWindow
            if (parent != null) {
                var item = new Item();
                item.OnSelect += OnSelect;
                Items.Add(item);
            }

            Fill();
        }

        private void OnSelect(Item sender) {
            Selected = sender.Root;
            DialogResult = true;
            Close();
        }

        private bool IsAllow(object obj) {
            switch (obj) {
                case Namespace space:
                    if (CallerParent != null && Equals(space, CallerParent))
                        return false;
                    if (Equals(space, Caller))
                        return false;
                    break;
                case Struct st:
                    if (CallerParent != null && Equals(st, CallerParent))
                        return false;
                    if (Equals(st, Caller))
                        return false;
                    break;
                case StructStruct ss:
                    if (CallerParent != null && Equals(ss, CallerParent))
                        return false;
                    if (Equals(ss, Caller))
                        return false;
                    break;
            }
            return true;
        }

        private void StructStruct(StructStruct root) {
            foreach (var obj in root.Items) {
                if (obj is StructStruct st) {
                    if (IsAllow(obj)) {
                        var item = new Item(st);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }
                    if (!Equals(Caller, st))
                        StructStruct(st);
                }
            }
        }

        private void Struct(Struct root) {
            foreach (var obj in root.Items) {
                if (obj is StructStruct st) {
                    if (IsAllow(obj)) {
                        var item = new Item(st);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }
                    if (!Equals(Caller, st))
                        StructStruct(st);
                }
            }
        }

        private void Namespace(Namespace root) {
            foreach (var obj in root.Items) {
                if (obj is Namespace space) {
                    if (IsAllow(obj)) {
                        var item = new Item(space);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }
                    if (!Equals(Caller, space))
                        Namespace(space);
                } else if (!(Caller is Namespace) && obj is Struct st) {
                    if (IsAllow(obj)) {
                        var item = new Item(st);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }
                    if (!Equals(Caller, st))
                        Struct(st);
                }
            }
        }

        private void Fill() {
            foreach (var obj in Constants.MainWindow.Items) {
                if (obj is Namespace space) {
                    if (IsAllow(obj)) {
                        var item = new Item(space);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }
                    if (!Equals(Caller, space))
                        Namespace(space);
                } else if (obj is Struct st) {
                    if (IsAllow(obj)) {
                        var item = new Item(st);
                        item.OnSelect += OnSelect;
                        Items.Add(item);
                    }

                    if (!Equals(Caller, st))
                        Struct(st);
                }
            }
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e) {
            if (String.IsNullOrEmpty(searchBox.Text)) {
                items.ItemsSource = Items;
            } else {
                var search = searchBox.Text;
                var list = Items.Where(x => x.PtrPath.StartsWith(search));
                SearchItems.Clear();
                foreach (var item in list)
                    SearchItems.Add(item);
                items.ItemsSource = SearchItems;
            }
        }
    }
}