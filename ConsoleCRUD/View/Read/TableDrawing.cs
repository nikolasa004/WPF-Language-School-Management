using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.View.Read
{
    // Handles the drawing of parts of a table for console output
    internal class TableDrawing
    {
        private const int MinColumnWidth = 10;
        private const int MaxColumnWidth = 32;
        private const int CellPadding = 1;

        // Represents a page of columns to be drawn together
        internal sealed class Page { public int Start; public int Count; }

        public static int[] ComputeColumnWidths(List<Column> cols, List<string[]> data)
        {
            var w = new int[cols.Count];
            for (int c = 0; c < cols.Count; c++)
            {
                int width = Clamp(cols[c].Header.Length);
                foreach (var r in data) width = Math.Max(width, Clamp(r[c]?.Length ?? 0));
                w[c] = width;
            }
            return w;
        }

        public static List<Page> BuildColumnPages(int[] widths)
        {
            var pages = new List<Page>();
            int totalWidth = Math.Max(30, Console.WindowWidth - 2);
            int i = 0;
            while (i < widths.Length)
            {
                int used = 1, start = i, count = 0;
                while (i < widths.Length)
                {
                    int cell = widths[i] + (CellPadding * 2) + 1;
                    if (used + cell + 1 > totalWidth) break;
                    used += cell; i++; count++;
                }
                if (count == 0) { count = 1; i = start + 1; }
                pages.Add(new Page { Start = start, Count = count });
            }
            return pages;
        }

        public static void DrawPage(List<Column> cols, List<string[]> data, int[] widths, Page page, RenderOptions options)
        {
            var idxs = Enumerable.Range(page.Start, page.Count).ToArray();

            ThemedConsole.With(options.Theme, options.Theme.Header, () =>
            {
                options.Skin.Top(SelectWidths(widths, idxs));
                options.Skin.RowStart();
                foreach (var idx in idxs)
                    options.Skin.Cell(cols[idx].Header, widths[idx], alignRight: false);
                options.Skin.RowEnd();
                options.Skin.Mid(SelectWidths(widths, idxs));
            });

            ThemedConsole.With(options.Theme, options.Theme.Value, () =>
            {
                foreach (var r in data)
                {
                    options.Skin.RowStart();
                    foreach (var idx in idxs)
                        options.Skin.Cell(r[idx] ?? "", widths[idx], cols[idx].AlignRight);
                    options.Skin.RowEnd();
                }
                options.Skin.Bot(SelectWidths(widths, idxs));
            });
        }

        private static IReadOnlyList<int> SelectWidths(int[] widths, int[] idxs)
        {
            var list = new List<int>(idxs.Length);
            foreach (var i in idxs) list.Add(widths[i]);
            return list;
        }

        public static string Trunc(string s, int width)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Length <= width) return s;
            return s.Substring(0, Math.Max(1, width - 1)) + "…";
        }

        public static int Clamp(int n)
        {
            return Math.Max(MinColumnWidth, Math.Min(n, MaxColumnWidth));
        }
    }
}
