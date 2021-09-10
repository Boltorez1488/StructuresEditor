using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuresEditor {
    internal class EmptyPreCreator {
        public static void Struct(List<string> build, Struct root, int tabsCount = 0) {
            var tab = Compiler.Tab(tabsCount);
            build.Add($"{tab}struct {root.MainName};");
        }

        public static void Enum(List<string> build, Enum root, int tabsCount = 0) {
            var tab = Compiler.Tab(tabsCount);
            build.Add($"{tab}enum {root.MainName};");
        }

        public static void Namespace(List<string> build, Namespace root, int tabsCount = 0) {
            var tab = Compiler.Tab(tabsCount);
            if (root.Items.Count != 0) {
                build.Add($"{tab}namespace {root.MainName} {{");
                foreach (var item in root.Items) {
                    switch (item) {
                        case Namespace space:
                            Namespace(build, space, tabsCount + 1);
                            break;
                        case Enum en:
                            Enum(build, en, tabsCount + 1);
                            break;
                        case Struct st:
                            Struct(build, st, tabsCount + 1);
                            break;
                    }
                }
                build.Add($"{tab}}}");
            }
        }

        public static void SearchInStruct(List<Struct> structs, StructStruct root) {
            foreach (var item in root.Items) {
                switch (item) {
                    case StructStruct ss:
                        SearchInStruct(structs, ss);
                        break;
                    case StructPtr ptr:
                        if (!SizeCompiler.IsPtr(ptr.Variable)) {
                            structs.Add(ptr.Root as Struct);
                        }
                        break;
                }
            }
        }

        public static void SearchInStruct(List<Struct> structs, Struct root) {
            foreach (var item in root.Items) {
                switch (item) {
                    case StructStruct ss:
                        SearchInStruct(structs, ss);
                        break;
                    case StructPtr ptr:
                        if (!SizeCompiler.IsPtr(ptr.Variable)) {
                            structs.Add(ptr.Root as Struct);
                        }
                        break;
                }
            }
        }

        public static void SearchInNamespace(List<Struct> structs, Namespace root) {
            foreach (var item in root.Items) {
                switch (item) {
                    case Namespace space:
                        SearchInNamespace(structs, space);
                        break;
                    case Struct st:
                        SearchInStruct(structs, st);
                        break;
                }
            }
        }

        public static List<Struct> SearchNoPtrs() {
            List<Struct> structs = new List<Struct>();
            foreach (var item in Constants.MainWindow.Items) {
                switch (item) {
                    case Namespace space:
                        SearchInNamespace(structs, space);
                        break;
                    case Struct st:
                        SearchInStruct(structs, st);
                        break;
                }
            }
            return structs;
        }

        public static void Create(List<string> build) {
            if (Constants.MainWindow.Items.Count == 0)
                return;

            if (!string.IsNullOrEmpty(Constants.MainWindow.GlobalNamespace)) {
                build.Add($"namespace {Constants.MainWindow.GlobalNamespace} {{");
            }

            var last = Constants.MainWindow.Items.Last();
            foreach (var item in Constants.MainWindow.Items) {
                switch (item) {
                    case Namespace space:
                        Namespace(build, space);
                        break;
                    case Enum en:
                        Enum(build, en);
                        break;
                    case Struct st:
                        Struct(build, st);
                        break;
                }
                //if (last != item)
                //    build.Add("");
            }

            if (!string.IsNullOrEmpty(Constants.MainWindow.GlobalNamespace)) {
                build.Add("}");
            }
        }
    }
}
