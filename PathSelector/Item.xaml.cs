using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StructuresEditor.PathSelector {
    public partial class Item : INotifyPropertyChanged {
        public delegate void SelectedEvent(Item sender);
        public event SelectedEvent OnSelect;
        public object Root;

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

        private void SetName(string parent, string name, string del = ".") {
            nameBlock.Text = "";
            if (parent == null) {
                PtrPath = name;

                TextRange mainName = new TextRange(nameBlock.ContentEnd, nameBlock.ContentEnd) { Text = name };
                mainName.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Root is Struct ? Constants.StructColor : Constants.EnumColor));
            } else {
                PtrPath = parent + del + name;

                var parentPath = new TextRange(nameBlock.ContentEnd, nameBlock.ContentEnd) { Text = parent + del };
                parentPath.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.LightBlue);

                TextRange mainName = new TextRange(nameBlock.ContentEnd, nameBlock.ContentEnd) { Text = name };
                mainName.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Root is Struct ? Constants.StructColor : Constants.EnumColor));
            }
        }

        public Item(Struct st) {
            InitializeComponent();
            DataContext = this;

            Root = st;
            if (st.ParentPath is string parent) {
                SetName(st.MainParent != null && parent.Length != 0 ? parent : null, st.MainName);
            }

            prefix.Text = "STRUCT";
            PrefixColor = Constants.StructColor;
        }

        public Item(Enum en) {
            InitializeComponent();
            DataContext = this;

            Root = en;
            if (en.ParentPath is string parent) {
                SetName(en.MainParent != null && parent.Length != 0 ? parent : null, en.MainName);
            }

            prefix.Text = "ENUM";
            PrefixColor = Constants.EnumColor;
        }

        public void SetCurrent() {
            prefix.Text = "CURRENT";
            PrefixColor = Colors.YellowGreen;
        }

        private void grid_MouseDown(object sender, MouseButtonEventArgs e) {
            OnSelect?.Invoke(this);
        }
    }
}
