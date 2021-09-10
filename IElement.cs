namespace StructuresEditor {
    public interface IElement {
        object MainParent { get; set; }
        string ParentPath { get; set; }
        string MainName { get; set; }
    }
}