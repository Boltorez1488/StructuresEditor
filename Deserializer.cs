using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace StructuresEditor {
    internal class Deserializer {
        struct Ptr {
            public readonly XElement Root;
            public readonly IStruct Parent;

            public Ptr(XElement root, IStruct parent) {
                Root = root;
                Parent = parent;
            }
        }
        private static readonly Queue<Ptr> Ptrs = new Queue<Ptr>();

        private static void StructVar(XElement root, IStruct parent) {
            var var = root.Value;
            var offset = root.Attribute(SerializerData.Offset)?.Value;
            var size = root.Attribute(SerializerData.Size)?.Value;
            var arraySize = root.Attribute(SerializerData.ArraySize)?.Value;

            var v = parent.AddVar();
            v.Variable = var;
            if (offset != null)
                v.Offset = int.Parse(offset);
            if (size != null)
                v.Size = int.Parse(size);
            if (arraySize != null)
                v.ArraySize = int.Parse(arraySize);
            parent.Sort();
        }

        private static void StructPtr(XElement root, IStruct parent) {
            var path = root.Attribute(SerializerData.Path)?.Value;
            if (string.IsNullOrEmpty(path))
                return;
            Ptrs.Enqueue(new Ptr(root, parent));
        }

        private static void StructEnum(XElement root, IStruct parent) {
            var name = root.Attribute(SerializerData.Name)?.Value;
            var var = root.Attribute(SerializerData.Var)?.Value;
            var offset = root.Attribute(SerializerData.Offset)?.Value;
            var size = root.Attribute(SerializerData.Size)?.Value;
            var arraySize = root.Attribute(SerializerData.ArraySize)?.Value;

            var en = parent.AddEnum();
            if (name != null)
                en.MainName = name;
            en.Variable = var;
            if (offset != null)
                en.Offset = int.Parse(offset);
            if (size != null)
                en.Size = int.Parse(size);
            if (arraySize != null)
                en.ArraySize = int.Parse(arraySize);

            foreach (XElement el in root.Elements()) {
                if (el.Name.LocalName == SerializerFields.EnumField) {
                    EnumField(el, en);
                }
            }

            var isExpand = root.Attribute(SerializerData.IsExpand)?.Value;
            if (isExpand != null) {
                en.expander.IsExpanded = bool.Parse(isExpand);
            }
            parent.Sort();
        }

        private static void StructStruct(XElement root, IStruct parent) {
            var name = root.Attribute(SerializerData.Name)?.Value;
            var var = root.Attribute(SerializerData.Var)?.Value;
            var offset = root.Attribute(SerializerData.Offset)?.Value;
            var size = root.Attribute(SerializerData.Size)?.Value;
            var arraySize = root.Attribute(SerializerData.ArraySize)?.Value;

            var cur = parent.AddStruct();
            if (name != null)
                cur.MainName = name;
            cur.Variable = var;
            if (offset != null)
                cur.Offset = int.Parse(offset);
            if (size != null)
                cur.Size = int.Parse(size);
            if (arraySize != null)
                cur.ArraySize = int.Parse(arraySize);

            foreach (XElement el in root.Elements()) {
                switch (el.Name.LocalName) {
                    case SerializerFields.StructVar:
                        StructVar(el, cur);
                        break;
                    case SerializerFields.StructPtr:
                        StructPtr(el, cur);
                        break;
                    case SerializerFields.StructEnum:
                        StructEnum(el, cur);
                        break;
                    case SerializerFields.StructStruct:
                        StructStruct(el, cur);
                        break;
                }
            }

            var isExpand = root.Attribute(SerializerData.IsExpand)?.Value;
            if (isExpand != null) {
                cur.expander.IsExpanded = bool.Parse(isExpand);
            }
            parent.Sort();
        }

        private static void Struct(XElement root, Namespace parent) {
            var cur = parent == null ? Constants.MainWindow.AddStruct() : parent.AddStruct();
            cur.MainName = root.Attribute(SerializerData.Name)?.Value;
            foreach (XElement el in root.Elements()) {
                switch (el.Name.LocalName) {
                    case SerializerFields.StructVar:
                        StructVar(el, cur);
                        break;
                    case SerializerFields.StructPtr:
                        StructPtr(el, cur);
                        break;
                    case SerializerFields.StructEnum:
                        StructEnum(el, cur);
                        break;
                    case SerializerFields.StructStruct:
                        StructStruct(el, cur);
                        break;
                }
            }

            var isExpand = root.Attribute(SerializerData.IsExpand)?.Value;
            if (isExpand != null) {
                cur.expander.IsExpanded = bool.Parse(isExpand);
            }
        }

        private static void EnumField(XElement root, IEnum parent) {
            var field = root.Attribute(SerializerData.Name)?.Value;
            var val = root.Value;

            var en = parent.AddField();
            en.Field = field;
            en.MainValue = int.Parse(val);
        }

        private static void Enum(XElement root, Namespace parent) {
            var cur = parent == null ? Constants.MainWindow.AddEnum() : parent.AddEnum();
            cur.MainName = root.Attribute(SerializerData.Name)?.Value;
            foreach (XElement el in root.Elements()) {
                if (el.Name.LocalName == SerializerFields.EnumField) {
                    EnumField(el, cur);
                }
            }

            var isExpand = root.Attribute(SerializerData.IsExpand)?.Value;
            if (isExpand != null) {
                cur.expander.IsExpanded = bool.Parse(isExpand);
            }
        }

        private static void Namespace(XElement root, Namespace parent) {
            var cur = parent == null ? Constants.MainWindow.AddNamespace() : parent.AddNamespace();
            cur.MainName = root.Attribute(SerializerData.Name)?.Value;
            foreach (XElement el in root.Elements()) {
                switch (el.Name.LocalName) {
                    case SerializerFields.Namespace:
                        Namespace(el, cur);
                        break;
                    case SerializerFields.Struct:
                        Struct(el, cur);
                        break;
                    case SerializerFields.Enum:
                        Enum(el, cur);
                        break;
                }
            }

            var isExpand = root.Attribute(SerializerData.IsExpand)?.Value;
            if (isExpand != null) {
                cur.expander.IsExpanded = bool.Parse(isExpand);
            }
        }

        private static void FillPtrs() {
            while (Ptrs.Count != 0) {
                var ptr = Ptrs.Dequeue();
                var root = ptr.Root;
                var path = root.Attribute(SerializerData.Path)?.Value;
                var found = Utils.FindPath(path);
                if (found != null) {
                    switch (found) {
                        case Struct st:
                            var var = root.Value;
                            var offset = root.Attribute(SerializerData.Offset)?.Value;
                            var size = root.Attribute(SerializerData.Size)?.Value;
                            var arraySize = root.Attribute(SerializerData.ArraySize)?.Value;

                            var cur = ptr.Parent.AddPtr(found);
                            cur.PtrPath = path;
                            cur.Variable = var;
                            if (offset != null)
                                cur.Offset = int.Parse(offset);
                            if (size != null)
                                cur.Size = int.Parse(size);
                            if (arraySize != null)
                                cur.ArraySize = int.Parse(arraySize);
                            break;
                        case Enum en:
                            var = root.Value;
                            offset = root.Attribute(SerializerData.Offset)?.Value;
                            size = root.Attribute(SerializerData.Size)?.Value;
                            arraySize = root.Attribute(SerializerData.ArraySize)?.Value;

                            cur = ptr.Parent.AddPtr(found);
                            cur.Variable = var;
                            if (offset != null)
                                cur.Offset = int.Parse(offset);
                            if (size != null)
                                cur.Size = int.Parse(size);
                            if (arraySize != null)
                                cur.ArraySize = int.Parse(arraySize);
                            break;
                    }
                    ptr.Parent.Sort();
                }
            }
        }

        public static void Load(string fin) {
            if (!File.Exists(fin))
                return;
            var xDoc = XDocument.Load(fin);
            var root = xDoc.Root;
            if (root == null)
                return;
            Constants.MainWindow.Items.Clear();
            foreach (XElement el in root.Elements()) {
                switch (el.Name.LocalName) {
                    case SerializerFields.Namespace:
                        Namespace(el, null);
                        break;
                    case SerializerFields.Struct:
                        Struct(el, null);
                        break;
                    case SerializerFields.Enum:
                        Enum(el, null);
                        break;
                }
            }
            FillPtrs();
        }
    }
}