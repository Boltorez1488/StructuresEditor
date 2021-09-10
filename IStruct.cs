namespace StructuresEditor {
    public interface IStruct {
        StructEnum AddEnum();
        StructPtr AddPtr(object obj);
        StructStruct AddStruct();
        StructVar AddVar();

        void Sort();
    }
}