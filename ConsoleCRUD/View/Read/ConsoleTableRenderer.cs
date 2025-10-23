using ConsoleCRUD.Services.Read;

namespace ConsoleCRUD.View.Read
{
    //Main class for rendering tabular data in the console with pagination and detail view. Uses ColumnModel, TableDrawing, and DetailedView.
    public class ConsoleTableRenderer
    {
        private const int MaxDepthDefault = 2;

        public static void Render(Type modelType, IEnumerable<object> items, int maxDepth = MaxDepthDefault)
        {
            var rows = SafeList(items);
            if (rows.Count == 0) { Color.WriteInfo($"(nema stavki za {modelType.Name})"); return; }

            var columns = ColumnModel.BuildColumns(modelType, maxDepth);
            var data = ColumnModel.Materialize(rows, columns);
            var widths = TableDrawing.ComputeColumnWidths(columns, data);
            var pages = TableDrawing.BuildColumnPages(widths);

            RunLoop(modelType, rows, columns, data, widths, pages, maxDepth);
        }

        private static List<object> SafeList(IEnumerable<object> items)
        {
            return items?.ToList() ?? new List<object>();
        }

        private static void RunLoop(Type modelType, List<object> rows,List<Column> cols,List<string[]> data,int[] widths,List<TableDrawing.Page> pages,int maxDepth)
        {
            int page = 0;
            while (true)
            {
                Console.Clear();
                TableDrawing.DrawPage(cols, data, widths, pages[page]);
                DrawHelpBar(page, pages.Count, rows.Count);

                var key = ReadCommand();
                if (key == ConsoleKey.Q) break;
                if (key == ConsoleKey.N && page < pages.Count - 1) { page++; continue; }
                if (key == ConsoleKey.P && page > 0) { page--; continue; }
                if (key == ConsoleKey.V) ShowDetail(modelType, rows, maxDepth);
            }
        }

        private static void DrawHelpBar(int page, int totalPages, int totalRows)
        {
            Color.With(ConsoleColor.DarkCyan, () =>
            {
                Console.Write(" [");
                Color.With(ConsoleColor.Yellow, () => Console.Write("N"));
                Console.Write("=Next  ");
                Color.With(ConsoleColor.Yellow, () => Console.Write("P"));
                Console.Write("=Prev  ");
                Color.With(ConsoleColor.Yellow, () => Console.Write("V"));
                Console.Write("=View  ");
                Color.With(ConsoleColor.Yellow, () => Console.Write("Q"));
                Console.Write("=Quit]  ");
                Console.WriteLine($"Strana: {page + 1}/{totalPages}  |  Redova: {totalRows}");
            });
        }

        private static ConsoleKey ReadCommand()
        {
            Console.Write("Komanda: ");
            return Console.ReadKey(intercept: true).Key;
        }

        private static void ShowDetail(Type modelType, List<object> rows, int maxDepth)
        {
            Console.Write($"Red # (1..{rows.Count}): ");
            var s = Console.ReadLine();
            if (!int.TryParse(s, out var ix) || ix < 1 || ix > rows.Count)
            {
                Color.WriteWarn("Nevažeći broj reda. Enter za nastavak...");
                Console.ReadLine();
                return;
            }
            DetailedView.RenderDetail(modelType, rows[ix - 1], Math.Max(3, maxDepth + 1));
        }
    }

    //Used to set up colored console output
    internal static class Color
    {
        public static void With(ConsoleColor c, Action act)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = c;
            act();
            Console.ForegroundColor = old;
        }

        public static void WriteInfo(string s) => With(ConsoleColor.DarkCyan, () => Console.WriteLine(s));
        public static void WriteWarn(string s) => With(ConsoleColor.Yellow, () => Console.WriteLine(s));
    }
}