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

namespace StructuresEditor.PathSelector {
    /// <summary>
    /// Логика взаимодействия для MainDialog.xaml
    /// </summary>
    public partial class MainDialog : Window {
        public ObservableCollection<Item> Items = new ObservableCollection<Item>();
        public ObservableCollection<Item> SearchItems = new ObservableCollection<Item>();
        public object Selected;
        public object Caller;

        public MainDialog(object caller = null) {
            InitializeComponent();
            Caller = caller;
            DataContext = this;
            items.ItemsSource = Items;

            Fill();
        }

        private void OnSelect(Item sender) {
            Selected = sender.Root;
            DialogResult = true;
            Close();
        }

        private void Namespace(Namespace root) {
            foreach (var obj in root.Items) {
                if(obj is Namespace space)
                    Namespace(space);
                else if (obj is Struct st) {
                    var item = new Item(st);
                    item.OnSelect += OnSelect;
                    if (Caller != null && Equals(st, Caller)) {
                        item.SetCurrent();
                        Items.Insert(0, item);
                    } else {
                        Items.Add(item);
                    }
                    
                } else if (obj is Enum en) {
                    var item = new Item(en);
                    item.OnSelect += OnSelect;
                    Items.Add(item);
                }
            }
        }

        private void Fill() {
            foreach (var obj in Constants.MainWindow.Items) {
                if (obj is Namespace space) {
                    Namespace(space);
                } else if (obj is Struct st) {
                    var item = new Item(st);
                    item.OnSelect += OnSelect;
                    if (Caller != null && Equals(st, Caller)) {
                        item.SetCurrent();
                        Items.Insert(0, item);
                    } else {
                        Items.Add(item);
                    }

                } else if (obj is Enum en) {
                    var item = new Item(en);
                    item.OnSelect += OnSelect;
                    Items.Add(item);
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
                foreach(var item in list)
                    SearchItems.Add(item);
                items.ItemsSource = SearchItems;
            }
        }
    }
}
