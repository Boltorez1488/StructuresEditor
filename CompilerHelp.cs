namespace StructuresEditor {
    struct EnumFieldParam {
        public EnumField Root;
        public bool Last;

        public EnumFieldParam(EnumField root, bool last) {
            Root = root;
            Last = last;
        }
    }

    struct StructVarParam {
        public IStructField Preview;
        public StructVar Current;

        public StructVarParam(IStructField prev, StructVar cur) {
            Preview = prev;
            Current = cur;
        }
    }

    struct StructPtrParam {
        public IStructField Preview;
        public StructPtr Current;

        public StructPtrParam(IStructField prev, StructPtr cur) {
            Preview = prev;
            Current = cur;
        }
    }

    struct StructEnumParam {
        public IStructField Preview;
        public StructEnum Current;

        public StructEnumParam(IStructField prev, StructEnum cur) {
            Preview = prev;
            Current = cur;
        }
    }

    struct StructStructParam {
        public IStructField Preview;
        public StructStruct Current;

        public StructStructParam(IStructField prev, StructStruct cur) {
            Preview = prev;
            Current = cur;
        }
    }
}