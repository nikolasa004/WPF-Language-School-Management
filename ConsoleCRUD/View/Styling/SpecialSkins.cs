namespace ConsoleCRUD.View.Styling
{
    // ! skin
    public sealed class ExclaimEqualSkin : ITableSkin
    {
        private const int Pad = 1;
        public void Top(IReadOnlyList<int> widths) => Sep(widths);
        public void Mid(IReadOnlyList<int> widths) => Sep(widths);
        public void Bot(IReadOnlyList<int> widths) => Sep(widths);

        public void RowStart() => Console.Write("!");
        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width); var pad = width - t.Length;
            if (alignRight) Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write("!");
        }
        public void RowEnd() => Console.WriteLine();

        private static void Sep(IReadOnlyList<int> widths)
        {
            Console.Write("!");
            foreach (var w in widths) { Console.Write(new string('=', w + 2 * Pad)); Console.Write("!"); }
            Console.WriteLine();
        }
        private static string Trunc(string s, int w) => string.IsNullOrEmpty(s) ? "" : (s.Length <= w ? s : s[..Math.Max(1, w - 1)] + "…");
    }

    // ~ skin
    public sealed class TildeWaveSkin : ITableSkin
    {
        private const int Pad = 1;
        public void Top(IReadOnlyList<int> widths) => Wave(widths);
        public void Mid(IReadOnlyList<int> widths) => Wave(widths);
        public void Bot(IReadOnlyList<int> widths) => Wave(widths);

        public void RowStart() => Console.Write(":");
        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width); var pad = width - t.Length;
            if (alignRight) Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write(":");
        }
        public void RowEnd() => Console.WriteLine();

        private static void Wave(IReadOnlyList<int> widths)
        {
            Console.Write("~");
            foreach (var w in widths) { Console.Write(new string('~', w + 2 * Pad)); Console.Write("~"); }
            Console.WriteLine();
        }
        private static string Trunc(string s, int w) => string.IsNullOrEmpty(s) ? "" : (s.Length <= w ? s : s[..Math.Max(1, w - 1)] + "…");
    }

    // 8 skin
    public sealed class ZenEightSkin : ITableSkin
    {
        private const int Pad = 1;
        public void Top(IReadOnlyList<int> widths) => Sep(widths);
        public void Mid(IReadOnlyList<int> widths) => Sep(widths);
        public void Bot(IReadOnlyList<int> widths) => Sep(widths);

        public void RowStart() => Console.Write("8");
        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width); var pad = width - t.Length;
            if (alignRight) Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write("8");
        }
        public void RowEnd() => Console.WriteLine();

        private static void Sep(IReadOnlyList<int> widths)
        {
            Console.Write("8");
            foreach (var w in widths) { Console.Write(new string('-', w + 2 * Pad)); Console.Write("8"); }
            Console.WriteLine();
        }
        private static string Trunc(string s, int w) => string.IsNullOrEmpty(s) ? "" : (s.Length <= w ? s : s[..Math.Max(1, w - 1)] + "…");
    }
}
