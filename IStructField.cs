namespace StructuresEditor {
    public interface IStructField {
        int ArraySize { get; set; }
        int FullSize { get; set; }
        int OldOffset { get; set; }
        int Offset { get; set; }
        int Size { get; set; }
        string Variable { get; set; }
    }
}