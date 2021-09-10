using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StructuresEditor {
    public class Utils {
        public static bool IsMatchRegex(string text, Regex regex) {
            var match = regex.Match(text);
            return match.Index == 0 && match.Length == text.Length;
        }

        public static int ToInt(string text) {
            if (IsMatchRegex(text, new Regex("0x[A-Fa-f0-9]{0,8}"))) {
                var toParse = text.Substring(2);
                return int.Parse(toParse, NumberStyles.HexNumber);
            }

            if (IsMatchRegex(text, new Regex("[A-Fa-f0-9]{1,8}h"))) {
                var toParse = text.TrimEnd('h');
                return int.Parse(toParse, NumberStyles.HexNumber);
            }

            if (IsMatchRegex(text, new Regex("[0-9]+"))) {
                return Convert.ToInt32(text);
            }

            return 0x0;
        }

        public static void EnterText(object textBox) {
            var tBox = textBox as TextBox;
            DependencyProperty prop = TextBox.TextProperty;

            BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
            binding?.UpdateSource();
        }

        private static object ExtractPath(Namespace root, string[] split) {
            foreach (var s in split) {
                var item = root.GetItem(s);
                switch (item) {
                    case Namespace space:
                        if (split.Length == 1)
                            return space;
                        return ExtractPath(space, split.Skip(1).ToArray());
                    case Struct st:
                        return st;
                    case Enum en:
                        return en;
                }
            }

            return null;
        }

        public static object FindPath(string path) {
            var split = path.Split('.', ':').Where(x => !String.IsNullOrEmpty(x)).ToArray();
            if (split.Length == 1) {
                return Constants.MainWindow.GetItem(split[0]);
            }
            var root = Constants.MainWindow.GetItem(split[0]);
            switch (root) {
                case Namespace space:
                    if (split.Length == 1)
                        return space;
                    return ExtractPath(space, split.Skip(1).ToArray());
                case Struct st:
                    return st;
                case Enum en:
                    return en;
            }
            return null;
        }

        public static string SymbolGenerate(char symbol, int count) {
            var result = "";
            while (count > 0) {
                result += symbol;
                count--;
            }
            return result;
        }

        public static int CalcWheel(int delta) {
            if (delta > 0) {
                if (Keyboard.IsKeyDown(Key.LeftAlt))
                    return 100;
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    return 8;
                if (Keyboard.IsKeyDown(Key.LeftShift))
                    return 4;
                return 1;
            }
            if (Keyboard.IsKeyDown(Key.LeftAlt))
                return -100;
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                return -8;
            if (Keyboard.IsKeyDown(Key.LeftShift))
                return -4;
            return -1;
        }
    }
}
