namespace ConsoleCRUD.View.Styling
{
    public interface ITableSkin
    {
        void Top(IReadOnlyList<int> widths);
        void Mid(IReadOnlyList<int> widths);
        void Bot(IReadOnlyList<int> widths);
        void RowStart();
        void Cell(string text, int width, bool alignRight);
        void RowEnd();
    }
}
