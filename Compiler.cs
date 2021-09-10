using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StructuresEditor {
    internal class Compiler {
        public static void SkipCalc(List<string> build, ref int skipCounter, string tab, IStructField prev, IStructField current) {
            if (prev != null) {
                var sub = current.Offset - (prev.Offset + prev.FullSize);
                if (sub > 0) {
                    build.Add(
                        skipCounter != 0 ?
                            $"{tab}BYTE _skip{skipCounter}[0x{sub:X}];" :
                            $"{tab}BYTE _skip[0x{sub:X}];"
                    );
                    skipCounter++;
                }
            } else if(current.Offset != 0x0) {
                build.Add(
                    skipCounter != 0 ?
                        $"{tab}BYTE _skip{skipCounter}[0x{current.Offset:X}];" :
                        $"{tab}BYTE _skip[0x{current.Offset:X}];"
                );
                skipCounter++;
            }
        }

        public static void StructVar(List<string> build, StructVarParam param, ref int skipCounter, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            var root = param.Current;
            var prev = param.Preview;

            SkipCalc(build, ref skipCounter, tab, prev, root);

            build.Add(Constants.MainWindow.PrintOffsets
                ? $"{tab}/* 0x{param.Current.Offset:X8} */ {root.Variable};"
                : $"{tab}{root.Variable};");
        }

        public static void StructPtr(List<string> build, StructPtrParam param, ref int skipCounter, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            var root = param.Current;
            var prev = param.Preview;

            SkipCalc(build, ref skipCounter, tab, prev, root);

            build.Add(Constants.MainWindow.PrintOffsets
                ? $"{tab}/* 0x{param.Current.Offset:X8} */ {root.PtrPath.Replace(".", "::")} {root.Variable};"
                : $"{tab}{root.PtrPath.Replace(".", "::")} {root.Variable};");
        }

        public static void StructEnum(List<string> build, StructEnumParam param, ref int skipCounter, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            var root = param.Current;
            var prev = param.Preview;

            SkipCalc(build, ref skipCounter, tab, prev, root);

            if (Constants.MainWindow.PrintOffsets)
                build.Add($"{tab}/* 0x{param.Current.Offset:X8} */");
            build.Add($"{tab}enum {root.MainName} {{");
            if (root.Items.Count != 0) {
                var last = root.Items.Last();
                foreach (var item in root.Items) {
                    EnumField(build, new EnumFieldParam(item, Equals(item, last)), tabsCount + 1);
                }
            }
            build.Add($"{tab}}} {root.Variable};");
        }

        public static void StructStruct(List<string> build, StructStructParam param, ref int skipCounter, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            var root = param.Current;
            var prev = param.Preview;

            SkipCalc(build, ref skipCounter, tab, prev, root);

            if (Constants.MainWindow.PrintOffsets)
                build.Add($"{tab}/* 0x{param.Current.Offset:X8} */");
            build.Add($"{tab}struct {root.MainName} {{");
            if (root.Items.Count != 0) {
                IStructField subPrev = null;
                var subSkipCounter = 0;
                foreach (var item in root.Items) {
                    switch (item) {
                        case StructVar var:
                            StructVar(build, new StructVarParam(subPrev, var), ref subSkipCounter, tabsCount + 1);
                            break;
                        case StructPtr ptr:
                            StructPtr(build, new StructPtrParam(subPrev, ptr), ref subSkipCounter, tabsCount + 1);
                            break;
                        case StructEnum sEn:
                            StructEnum(build, new StructEnumParam(subPrev, sEn), ref subSkipCounter, tabsCount + 1);
                            break;
                        case StructStruct ss:
                            StructStruct(build, new StructStructParam(subPrev, ss), ref subSkipCounter, tabsCount + 1);
                            break;
                    }
                    subPrev = item as IStructField;
                }
            }
            build.Add($"{tab}}} {root.Variable};");
        }

        public static void Struct(List<string> build, Struct root, int tabsCount = 0, bool standart = false) {
            var tab = Tab(tabsCount);
            if (!standart && EmptyCreator.NeedCompile.Contains(root)) {
                build.Add($"{tab}struct {root.MainName};");
                return;
            }
            build.Add($"{tab}struct {root.MainName} {{");
            if (root.Items.Count != 0) {
                IStructField prev = null;
                var skipCounter = 0;
                foreach (var item in root.Items) {
                    switch (item) {
                        case StructVar var:
                            StructVar(build, new StructVarParam(prev, var), ref skipCounter, tabsCount + 1);
                            break;
                        case StructPtr ptr:
                            StructPtr(build, new StructPtrParam(prev, ptr), ref skipCounter, tabsCount + 1);
                            break;
                        case StructEnum sEn:
                            StructEnum(build, new StructEnumParam(prev, sEn), ref skipCounter, tabsCount + 1);
                            break;
                        case StructStruct ss:
                            StructStruct(build, new StructStructParam(prev, ss), ref skipCounter, tabsCount + 1);
                            break;
                    }
                    prev = item as IStructField;
                }
            }
            build.Add($"{tab}}};");
        }

        public static void EnumField(List<string> build, EnumFieldParam param, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            var root = param.Root;
            build.Add(
                !param.Last ? 
                $"{tab}{root.Field} = {root.MainValue}," 
                : $"{tab}{root.Field} = {root.MainValue}"
            );
        }

        public static void Enum(List<string> build, Enum root, int tabsCount = 0) {
            var tab = Tab(tabsCount);
            build.Add($"{tab}enum {root.MainName} {{");
            if (root.Items.Count != 0) {
                var last = root.Items.Last();
                foreach (var item in root.Items) {
                    EnumField(build, new EnumFieldParam(item, Equals(item, last)), tabsCount + 1);
                }
            }
            build.Add($"{tab}}};");
        }

        public static void Namespace(List<string> build, Namespace root, int tabsCount = 0) {
            var tab = Tab(tabsCount);
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

        public static void Compile(string outName, string folder, string emptyFile = null) {
            if (Constants.MainWindow.Items.Count == 0)
                return;
            var build = string.IsNullOrEmpty(emptyFile) ? 
                new List<string> {"#pragma once", ""} :
                new List<string> { "#pragma once", $"#include \"{emptyFile}\"", "" };

            build.Add("#pragma pack(push,1)"); // Align 1 byte
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
                if(last != item)
                    build.Add("");
            }
            if (!string.IsNullOrEmpty(Constants.MainWindow.GlobalNamespace)) {
                build.Add("}");
            }
            build.Add("#pragma pack(pop)");

            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            Write(string.IsNullOrEmpty(folder) ? outName : $"{folder}/{outName}", build);
        }

        public static string Tab(int count = 0) {
            return Utils.SymbolGenerate('\t', count);
        }

        public static void Write(string fout, List<string> lines) {
            using (var stream = new StreamWriter(new FileStream(fout, FileMode.Create))) {
                for (var i = 0; i < lines.Count; i++)
                    if (i == lines.Count - 1)
                        stream.Write(lines[i]);
                    else
                        stream.WriteLine(lines[i]);
            }
        }
    }
}