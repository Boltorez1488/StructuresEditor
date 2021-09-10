using System.Xml.Linq;

namespace StructuresEditor {
    internal class SerializerFields {
        public const string Namespace = "Namespace";
        public const string Struct = "Struct";
        public const string Enum = "Enum";

        public const string EnumField = "Field";

        public const string StructVar = "Var";
        public const string StructPtr = "Ptr";
        public const string StructStruct = "Struct";
        public const string StructEnum = "Enum";
    }

    internal class SerializerData {
        public const string Serializator = "StructureEditor";
        public const string IsExpand = "IsExpand";

        public const string Offset = "Offset";
        public const string Size = "Size";
        public const string ArraySize = "ArraySize";
        public const string Path = "Path";
        public const string Name = "Name";
        public const string Var = "Var";
    }

    internal class Serializer {
        private static void StructVar(XElement root, StructVar var) {
            var el = new XElement(SerializerFields.StructVar, var.Variable);
            el.SetAttributeValue(SerializerData.Offset, var.Offset);
            el.SetAttributeValue(SerializerData.Size, var.Size);
            el.SetAttributeValue(SerializerData.ArraySize, var.ArraySize);
            root.Add(el);
        }

        private static void StructPtr(XElement root, StructPtr ptr) {
            var el = new XElement(SerializerFields.StructPtr, ptr.Variable);
            el.SetAttributeValue(SerializerData.Offset, ptr.Offset);
            el.SetAttributeValue(SerializerData.Size, ptr.Size);
            el.SetAttributeValue(SerializerData.ArraySize, ptr.ArraySize);
            el.SetAttributeValue(SerializerData.Path, ptr.PtrPath);
            root.Add(el);
        }

        private static void StructStruct(XElement root, StructStruct ss) {
            var el = new XElement(SerializerFields.StructStruct);
            el.SetAttributeValue(SerializerData.Offset, ss.Offset);
            el.SetAttributeValue(SerializerData.Size, ss.Size);
            el.SetAttributeValue(SerializerData.ArraySize, ss.ArraySize);
            el.SetAttributeValue(SerializerData.Name, ss.MainName);
            el.SetAttributeValue(SerializerData.Var, ss.Variable);
            root.Add(el);
            foreach (var item in ss.Items) {
                switch (item) {
                    case StructVar var:
                        StructVar(el, var);
                        break;
                    case StructPtr ptr:
                        StructPtr(el, ptr);
                        break;
                    case StructStruct st:
                        StructStruct(el, st);
                        break;
                    case StructEnum sEn:
                        StructEnum(el, sEn);
                        break;
                }
            }
            el.SetAttributeValue(SerializerData.IsExpand, ss.expander.IsExpanded);
        }

        private static void StructEnum(XElement root, StructEnum sEn) {
            var el = new XElement(SerializerFields.StructEnum);
            el.SetAttributeValue(SerializerData.Offset, sEn.Offset);
            el.SetAttributeValue(SerializerData.Size, sEn.Size);
            el.SetAttributeValue(SerializerData.ArraySize, sEn.ArraySize);
            el.SetAttributeValue(SerializerData.Name, sEn.MainName);
            el.SetAttributeValue(SerializerData.Var, sEn.Variable);
            root.Add(el);
            foreach (var item in sEn.Items) {
                EnumField(el, item);
            }
            el.SetAttributeValue(SerializerData.IsExpand, sEn.expander.IsExpanded);
        }

        private static void Struct(XElement root, Struct st) {
            var el = new XElement(SerializerFields.Struct);
            el.SetAttributeValue(SerializerData.Name, st.MainName);
            root.Add(el);
            foreach (var item in st.Items) {
                switch (item) {
                    case StructVar var:
                        StructVar(el, var);
                        break;
                    case StructPtr ptr:
                        StructPtr(el, ptr);
                        break;
                    case StructStruct ss:
                        StructStruct(el, ss);
                        break;
                    case StructEnum sEn:
                        StructEnum(el, sEn);
                        break;
                }
            }
            el.SetAttributeValue(SerializerData.IsExpand, st.expander.IsExpanded);
        }

        private static void EnumField(XElement root, EnumField field) {
            var el = new XElement(SerializerFields.EnumField, field.MainValue);
            el.SetAttributeValue(SerializerData.Name, field.Field);
            root.Add(el);
        }

        private static void Enum(XElement root, Enum en) {
            var el = new XElement(SerializerFields.Enum);
            el.SetAttributeValue(SerializerData.Name, en.MainName);
            root.Add(el);
            foreach (var item in en.Items) {
                EnumField(el, item);
            }
            el.SetAttributeValue(SerializerData.IsExpand, en.expander.IsExpanded);
        }

        private static void Namespace(XElement root, Namespace space) {
            var el = new XElement(SerializerFields.Namespace);
            el.SetAttributeValue(SerializerData.Name, space.MainName);
            root.Add(el);
            foreach (var item in space.Items) {
                switch (item) {
                    case Namespace child:
                        Namespace(el, child);
                        break;
                    case Struct child:
                        Struct(el, child);
                        break;
                    case Enum child:
                        Enum(el, child);
                        break;
                }
            }
            el.SetAttributeValue(SerializerData.IsExpand, space.expander.IsExpanded);
        }

        public static void Save(string fout) {
            if (Constants.MainWindow.Items.Count == 0)
                return;
            XDocument xDoc = new XDocument();
            XElement root = new XElement(SerializerData.Serializator);
            xDoc.Add(root);
            foreach (var item in Constants.MainWindow.Items) {
                switch (item) {
                    case Namespace child:
                        Namespace(root, child);
                        break;
                    case Struct child:
                        Struct(root, child);
                        break;
                    case Enum child:
                        Enum(root, child);
                        break;
                }
            }
            xDoc.Save(fout);
        }
    }
}