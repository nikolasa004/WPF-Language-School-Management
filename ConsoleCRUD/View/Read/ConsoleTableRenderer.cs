using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.View.Read
{
    //Main class for rendering tabular data in the console with pagination and detail view. Uses ColumnModel, TableDrawing, and DetailedView.
    public class ConsoleTableRenderer
    {
        private const int MaxDepthDefault = 2;

        public static void Render(Type modelType, IEnumerable<object> items, int maxDepth = MaxDepthDefault, RenderOptions? options = null)
        {
            options ??= RenderOptions.Default();

            var rows = SafeList(items);
            if (rows.Count == 0)
            {
                ThemedConsole.With(options.Theme, options.Theme.Info, () =>Console.WriteLine($"(nema stavki za {modelType.Name})"));
                return;
            }

            var columns = ColumnModel.BuildColumns(modelType, maxDepth);
            var data = ColumnModel.Materialize(rows, columns);
            var widths = TableDrawing.ComputeColumnWidths(columns, data);
            var pages = TableDrawing.BuildColumnPages(widths);

            RunLoop(modelType, rows, columns, data, widths, pages, maxDepth, options);
        }

        private static List<object> SafeList(IEnumerable<object> items)
        {
            return items?.ToList() ?? new List<object>();
        }

        private static void RunLoop(Type modelType, List<object> rows,List<Column> cols,List<string[]> data,int[] widths,List<TableDrawing.Page> pages,int maxDepth, RenderOptions options)
        {
            int page = 0;
            while (true)
            {
                Console.Clear();
                TableDrawing.DrawPage(cols, data, widths, pages[page], options);
                DrawHelpBar(page, pages.Count, rows.Count, options);

                var key = ReadCommand();
                if (key == ConsoleKey.Q) break;
                if (key == ConsoleKey.N && page < pages.Count - 1) { page++; continue; }
                if (key == ConsoleKey.P && page > 0) { page--; continue; }
                if (key == ConsoleKey.V) ShowDetail(modelType, rows, maxDepth, options);
            }
        }

        private static void DrawHelpBar(int page, int totalPages, int totalRows, RenderOptions options)
        {
            ThemedConsole.With(options.Theme, options.Theme.HelpBar, () =>
            {
                Console.Write(" [");
                ThemedConsole.With(options.Theme, options.Theme.CommandKey, () => Console.Write("N"));
                Console.Write("=Next  ");
                ThemedConsole.With(options.Theme, options.Theme.CommandKey, () => Console.Write("P"));
                Console.Write("=Prev  ");
                ThemedConsole.With(options.Theme, options.Theme.CommandKey, () => Console.Write("V"));
                Console.Write("=View  ");
                ThemedConsole.With(options.Theme, options.Theme.CommandKey, () => Console.Write("Q"));
                Console.Write("=Quit]  ");
                Console.WriteLine($"Strana: {page + 1}/{totalPages}  |  Redova: {totalRows}");
            });
        }

        private static ConsoleKey ReadCommand()
        {
            Console.Write("Komanda: ");
            return Console.ReadKey(intercept: true).Key;
        }

        private static void ShowDetail(Type modelType, List<object> rows, int maxDepth, RenderOptions options)
        {
            Console.Write($"Red # (1..{rows.Count}): ");
            var s = Console.ReadLine();
            if (!int.TryParse(s, out var ix) || ix < 1 || ix > rows.Count)
            {
                ThemedConsole.With(options.Theme, options.Theme.Warn, () =>Console.WriteLine("Nevažeći broj reda. Enter za nastavak..."));
                Console.ReadLine();
                return;
            }
            DetailedView.RenderDetail(modelType, rows[ix - 1], Math.Max(3, maxDepth + 1), options);
        }
    }
}