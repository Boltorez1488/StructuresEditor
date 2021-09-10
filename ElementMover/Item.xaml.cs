using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace StructuresEditor.ElementMover {
    public partial class Item : INotifyPropertyChanged {
        public delegate void SelectedEvent(Item sender);
        public event SelectedEvent OnSelect;
        public IElement Root;

        private Color _prefixColor;
        public Color PrefixColor {
            get => _prefixColor;
            set {
                _prefixColor = value;
                OnPropertyChanged();
            }
        }

        public string PtrPath;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public void AddText(string text, object color) {
            TextRange select = new TextRange(nameBlock.ContentEnd, nameBlock.ContentEnd) { Text = text };
            select.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private List<IElement> GetParts() {
            List<IElement> parts = new List<IElement>();
            if (Root is IElement root) {
                parts.Add(root);
                IElement parent = root.MainParent as IElement;
                while (parent != null) {
                    parts.Add(parent);
                    parent = parent.MainParent as IElement;
                }
            }
            parts.Reverse();
            return parts;
        }

        private void BuildPath() {
            var parts = GetParts();
            var last = parts.Last();
            foreach (var p in parts) {
                switch (p) {
                    case Namespace space:
                        AddText(space.MainName, new SolidColorBrush(Colors.LightBlue));
                        break;
                    case Struct st:
                        AddText(st.MainName, new SolidColorBrush(Constants.StructColor));
                        break;
                    case StructStruct ss:
                        AddText(ss.MainName, new SolidColorBrush(Constants.StructColor));
                        break;
                }
                if(p != last)
                    AddText(".", Brushes.White);
            }
        }

        private void SetName(string parent, string name, string del = ".") {
            nameBlock.Text = "";
            if (parent == null) {
                PtrPath = name;
                BuildPath();
            } else {
                PtrPath = parent + del + name;
                BuildPath();
            }
        }

        public Item(IElement root) {
            InitializeComponent();
            DataContext = this;

            Root = root;
            if (root.ParentPath is string parent) {
                SetName(root.MainParent != null && parent.Length != 0 ? parent : null, root.MainName);
            }

            switch (root) {
                case Namespace space:
                    prefix.Text = "NAMESPACE";
                    PrefixColor = Colors.LightBlue;
                    break;
                case Struct st:
                    prefix.Text = "STRUCT";
                    PrefixColor = Constants.StructColor;
                    break;
                case StructStruct ss:
                    prefix.Text = "STRUCT";
                    PrefixColor = Constants.StructColor;
                    break;
            }
        }

        public Item() {
            InitializeComponent();
            DataContext = this;

            Root = null;
            nameBlock.Text = "";
            TextRange mainName = new TextRange(nameBlock.ContentEnd, nameBlock.ContentEnd) { Text = "Root" };
            mainName.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.DarkOrange));

            prefix.Text = "WINDOW";
            PrefixColor = Colors.DarkOrange;
        }

        private void grid_MouseDown(object sender, MouseButtonEventArgs e) {
            OnSelect?.Invoke(this);
        }
    }
}