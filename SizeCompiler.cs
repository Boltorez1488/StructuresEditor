using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StructuresEditor {
    class SizeCompiler {
        public static int DefaultSize => Constants.MainWindow.Is64Bit ? 0x8 : 0x4;

        public static int ArrayExtract(string typename) {
            var startPos = typename.LastIndexOf("[", StringComparison.Ordinal);
            if (startPos <= 0) return DefaultSize;
            var val = typename.Substring(startPos + 1, typename.Length - startPos - 2);
            var result = Utils.ToInt(val);
            return result != 0x0 ? result : 0x1;
        }

        public static bool IsPtr(string typename) {
            return typename.IndexOf("*", StringComparison.Ordinal) != -1;
        }

        public static bool IsArray(string typename) {
            return typename.EndsWith("]");
        }

        public static int GetTypeSize(string typename) {
            if (IsPtr(typename))
                return DefaultSize;

            if (typename.StartsWith("signed ")) {
                typename = typename.Substring(7);
            } else if (typename.StartsWith("unsigned ")) {
                typename = typename.Substring(9);
            }

            if (typename.StartsWith("double") || typename.StartsWith("long long") || 
                typename.StartsWith("uint64") ||  typename.StartsWith("int64")) {
                return 0x8;
            }
            if (typename.StartsWith("short") || typename.StartsWith("uint16") || typename.StartsWith("int16")) {
                return 0x2;
            }
            if (typename.StartsWith("bool") || typename.StartsWith("char") || typename.StartsWith("byte") ||
                typename.StartsWith("uint8") || typename.StartsWith("int8")) {
                return 0x1;
            }

            return 0x4;
        }

        // Get size without array, only type
        public static int GetSize(string typename) {
            if (String.IsNullOrEmpty(typename)) {
                return DefaultSize;
            }

            if (IsPtr(typename))
                return DefaultSize;

            return GetTypeSize(typename);
        }
    }
}
