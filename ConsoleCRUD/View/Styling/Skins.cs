namespace ConsoleCRUD.View.Styling
{
    // ASCII (+---+ , | ...)
    public sealed class AsciiSkin : ITableSkin
    {
        private const int Pad = 1;

        public void Top(IReadOnlyList<int> widths) => Sep(widths);
        public void Mid(IReadOnlyList<int> widths) => Sep(widths);
        public void Bot(IReadOnlyList<int> widths) => Sep(widths);

        public void RowStart() => Console.Write("|");

        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width);
            var pad = width - t.Length;
            if (alignRight)
                Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else
                Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write("|");
        }

        public void RowEnd() => Console.WriteLine();

        private static void Sep(IReadOnlyList<int> widths)
        {
            Console.Write("+");
            foreach (var w in widths) { Console.Write(new string('-', w + 2 * Pad)); Console.Write("+"); }
            Console.WriteLine();
        }

        private static string Trunc(string s, int w)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Length <= w) return s;
            return s.Substring(0, Math.Max(1, w - 1)) + "…";
        }
    }

    // No borders, just spaces
    public sealed class MinimalSkin : ITableSkin
    {
        private const int Pad = 1;

        public void Top(IReadOnlyList<int> widths) { }
        public void Mid(IReadOnlyList<int> widths) { }
        public void Bot(IReadOnlyList<int> widths) { }

        public void RowStart() { }

        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width);
            var pad = width - t.Length;
            if (alignRight)
                Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else
                Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write(" ");
        }

        public void RowEnd() => Console.WriteLine();

        private static string Trunc(string s, int w)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Length <= w) return s;
            return s.Substring(0, Math.Max(1, w - 1)) + "…";
        }
    }

    // Dollar
    public sealed class DollarSkin : ITableSkin
    {
        private const int Pad = 1;
        public void Top(IReadOnlyList<int> widths) => Sep(widths, '$', '=');
        public void Mid(IReadOnlyList<int> widths) => Sep(widths, '$', '=');
        public void Bot(IReadOnlyList<int> widths) => Sep(widths, '$', '=');

        public void RowStart() => Console.Write("$");
        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width); var pad = width - t.Length;
            if (alignRight) Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write("$");
        }
        public void RowEnd() => Console.WriteLine();

        private static void Sep(IReadOnlyList<int> widths, char edge, char fill)
        {
            Console.Write(edge);
            foreach (var w in widths) { Console.Write(new string(fill, w + 2 * Pad)); Console.Write(edge); }
            Console.WriteLine();
        }
        private static string Trunc(string s, int w) => string.IsNullOrEmpty(s) ? "" : (s.Length <= w ? s : s[..Math.Max(1, w - 1)] + "…");
    }

    // Hash
    public sealed class HashStarSkin : ITableSkin
    {
        private const int Pad = 1;
        public void Top(IReadOnlyList<int> widths) => Bar(widths, '#');
        public void Mid(IReadOnlyList<int> widths) => Bar(widths, '*');
        public void Bot(IReadOnlyList<int> widths) => Bar(widths, '#');

        public void RowStart() => Console.Write("|");
        public void Cell(string text, int width, bool alignRight)
        {
            var t = Trunc(text, width); var pad = width - t.Length;
            if (alignRight) Console.Write(new string(' ', Pad + pad) + t + new string(' ', Pad));
            else Console.Write(new string(' ', Pad) + t + new string(' ', Pad + pad));
            Console.Write("|");
        }
        public void RowEnd() => Console.WriteLine();

        private static void Bar(IReadOnlyList<int> widths, char ch)
        {
            foreach (var w in widths) Console.Write(new string(ch, w + 2 * Pad + 1));
            Console.WriteLine(ch);
        }
        private static string Trunc(string s, int w) => string.IsNullOrEmpty(s) ? "" : (s.Length <= w ? s : s[..Math.Max(1, w - 1)] + "…");
    }
}
