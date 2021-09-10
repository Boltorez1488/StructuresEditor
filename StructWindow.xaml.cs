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
    public partial class StructWindow {
        public ObservableCollection<object> Items;// = new ObservableCollection<object>();
        public Struct Root;

        public StructWindow(Struct st) {
            InitializeComponent();

            Root = st;
            Root.items.ItemsSource = null;
            Items = Root.Items;
            items.ItemsSource = Items;

            Title = String.IsNullOrEmpty(st.ParentPath) ?
                $"Struct: {st.MainName}" : $"Struct: {st.ParentPath}.{st.MainName}";

            foreach (var item in Root.Items) {
                switch (item) {
                    case StructStruct obj:
                        obj.OnDelete -= Root.OnChildDelete;
                        obj.OnFullSizeChanged -= Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= Root.OnChildOffsetChanged;

                        obj.OnDelete += OnChildDelete;
                        obj.OnFullSizeChanged += OnChildFullSizeChanged;
                        obj.OnOffsetChanged += OnChildOffsetChanged;
                        break;
                    case StructEnum obj:
                        obj.OnDelete -= Root.OnChildDelete;
                        obj.OnFullSizeChanged -= Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= Root.OnChildOffsetChanged;

                        obj.OnDelete += OnChildDelete;
                        obj.OnFullSizeChanged += OnChildFullSizeChanged;
                        obj.OnOffsetChanged += OnChildOffsetChanged;
                        break;
                    case StructVar obj:
                        obj.OnDelete -= Root.OnChildDelete;
                        obj.OnFullSizeChanged -= Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= Root.OnChildOffsetChanged;

                        obj.OnDelete += OnChildDelete;
                        obj.OnFullSizeChanged += OnChildFullSizeChanged;
                        obj.OnOffsetChanged += OnChildOffsetChanged;
                        break;
                    case StructPtr obj:
                        obj.OnDelete -= Root.OnChildDelete;
                        obj.OnFullSizeChanged -= Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= Root.OnChildOffsetChanged;

                        obj.OnDelete += OnChildDelete;
                        obj.OnFullSizeChanged += OnChildFullSizeChanged;
                        obj.OnOffsetChanged += OnChildOffsetChanged;
                        break;
                }
            }
        }

        public void OnChildDelete(object sender) {
            if (Items.Count == 0) {
                return;
            }
            var index = Items.IndexOf(sender);
            if (index < 0)
                return;
            Items.RemoveAt(index);
            Root.ItemRemoved();
            if (Items.Count == 0) {
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

        public void AddVar() {
            var var = new StructVar();
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            Root.AddItem(var);
            Root.ItemAdded();
        }

        public void AddPtr() {
            var dlg = new PathSelector.MainDialog(Root);
            dlg.ShowDialog();
            if (dlg.DialogResult != true) return;
            switch (dlg.Selected) {
                case Struct st: {
                    var ptr = new StructPtr(st) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    Root.AddItem(ptr);
                    break;
                }
                case Enum en: {
                    var ptr = new StructPtr(en) { PtrParent = this };
                    ptr.OnDelete += OnChildDelete;
                    ptr.OnFullSizeChanged += OnChildFullSizeChanged;
                    ptr.OnOffsetChanged += OnChildOffsetChanged;
                    Root.AddItem(ptr);
                    break;
                }
            }
            Root.ItemAdded();
        }

        public void AddEnum() {
            var var = new StructEnum(Root);
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            Root.AddItem(var);
            Root.ItemAdded();
        }

        public void AddStruct() {
            var var = new StructStruct(Root);
            var.OnDelete += OnChildDelete;
            var.OnFullSizeChanged += OnChildFullSizeChanged;
            var.OnOffsetChanged += OnChildOffsetChanged;
            Root.AddItem(var);
            Root.ItemAdded();
        }

        public void OnChildOffsetChanged(object sender) {
            Struct.Sort(Items, Struct.Comparer);
        }

        public void OnChildFullSizeChanged(object sender) {
            Struct.Sort(Items, Struct.Comparer);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            items.Focus();
        }

        private void StructWindow_OnClosed(object sender, EventArgs e) {
            foreach (var item in Items) {
                switch (item) {
                    case StructStruct obj:
                        obj.OnDelete -= OnChildDelete;
                        obj.OnFullSizeChanged -= OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= OnChildOffsetChanged;

                        obj.OnDelete += Root.OnChildDelete;
                        obj.OnFullSizeChanged += Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += Root.OnChildOffsetChanged;
                        break;
                    case StructEnum obj:
                        obj.OnDelete -= OnChildDelete;
                        obj.OnFullSizeChanged -= OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= OnChildOffsetChanged;

                        obj.OnDelete += Root.OnChildDelete;
                        obj.OnFullSizeChanged += Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += Root.OnChildOffsetChanged;
                        break;
                    case StructVar obj:
                        obj.OnDelete -= OnChildDelete;
                        obj.OnFullSizeChanged -= OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= OnChildOffsetChanged;

                        obj.OnDelete += Root.OnChildDelete;
                        obj.OnFullSizeChanged += Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += Root.OnChildOffsetChanged;
                        break;
                    case StructPtr obj:
                        obj.OnDelete -= OnChildDelete;
                        obj.OnFullSizeChanged -= OnChildFullSizeChanged;
                        obj.OnOffsetChanged -= OnChildOffsetChanged;

                        obj.OnDelete += Root.OnChildDelete;
                        obj.OnFullSizeChanged += Root.OnChildFullSizeChanged;
                        obj.OnOffsetChanged += Root.OnChildOffsetChanged;
                        break;
                }
            }

            Root.dummy.Visibility = Items.Count != 0 ? Visibility.Collapsed : Visibility.Visible;

            items.ItemsSource = null;
            Root.items.ItemsSource = Root.Items;
        }

        private void AddVBtn_OnClick(object sender, RoutedEventArgs e) {
            AddVar();
        }

        private void AddPBtn_OnClick(object sender, RoutedEventArgs e) {
            AddPtr();
        }

        private void AddSBtn_OnClick(object sender, RoutedEventArgs e) {
            AddStruct();
        }

        private void AddEBtn_OnClick(object sender, RoutedEventArgs e) {
            AddEnum();
        }

        private void VarAdder(object sender, ExecutedRoutedEventArgs e) {
            AddVar();
        }

        private void PtrAdder(object sender, ExecutedRoutedEventArgs e) {
            AddPtr();
        }

        private void EnumAdder(object sender, ExecutedRoutedEventArgs e) {
            AddEnum();
        }

        private void StructAdder(object sender, ExecutedRoutedEventArgs e) {
            AddStruct();
        }
    }
}
