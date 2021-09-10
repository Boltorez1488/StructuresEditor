using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StructuresEditor {
    internal class EmptyCreator {
        public static List<Struct> NeedCompile = new List<Struct>();

        public static bool Struct(List<string> build, Struct root, int tabsCount = 0) {
            if (NeedCompile.Contains(root)) {
                Compiler.Struct(build, root, tabsCount, true);
                return true;
            }
            return false;
            //var tab = Compiler.Tab(tabsCount);
            //build.Add($"{tab}struct {root.MainName};");
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

        public static void Create(string emptyFile, string folder) {
            if (Constants.MainWindow.Items.Count == 0)
                return;

            var build = new List<string> {
                "#pragma once",
            };

            var add = Constants.MainWindow.AdditonalFile;
            if (!string.IsNullOrEmpty(add)) {
                build.Add($"#include \"{add}\"");
            }

            build.AddRange(new [] {
                "#include <Windows.h>",
                "#include <stdint.h>",
                "",
                "typedef int64_t int64;",
                "typedef int32_t int32;",
                "typedef int16_t int16;",
                "typedef int8_t int8;",
                "typedef uint64_t uint64;",
                "typedef uint32_t uint32;",
                "typedef uint16_t uint16;",
                "typedef uint8_t uint8;",
                ""
            });

            EmptyPreCreator.Create(build);
            build.Add("");

            NeedCompile = SearchNoPtrs();
            if (NeedCompile.Count > 0) {
                build.Add("#pragma pack(push,1)"); // Align 1 byte
                if (!string.IsNullOrEmpty(Constants.MainWindow.GlobalNamespace)) {
                    build.Add($"namespace {Constants.MainWindow.GlobalNamespace} {{");
                }
                var last = Constants.MainWindow.Items.Last();
                foreach (var item in Constants.MainWindow.Items) {
                    bool flag = true;
                    switch (item) {
                        case Namespace space:
                            Namespace(build, space);
                            break;
                        case Enum en:
                            Enum(build, en);
                            break;
                        case Struct st:
                            flag = Struct(build, st);
                            break;
                    }
                    if (flag && last != item)
                        build.Add("");
                }
                if (string.IsNullOrEmpty(build.Last())) {
                    build.RemoveAt(build.Count - 1);
                }
                if (!string.IsNullOrEmpty(Constants.MainWindow.GlobalNamespace)) {
                    build.Add("}");
                }
                build.Add("#pragma pack(pop)");
            }

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllLines(string.IsNullOrEmpty(folder) ? emptyFile : $"{folder}/{emptyFile}", build);
        }
    }
}