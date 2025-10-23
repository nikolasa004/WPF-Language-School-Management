using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.View
{
    //Contains helper methods for console UI rendering.
    internal static class ConsoleUI
    {
        public static void ClearWithHeader(string title, RenderOptions options)
        {
            Console.Clear();
            var line = new string('─', Math.Max(10, Math.Min(Console.WindowWidth - 4, 72)));
            Console.WriteLine($"┌{line}┐");
            ThemedConsole.With(options.Theme, options.Theme.Header, () => WriteCentered(title, options));
            Console.WriteLine($"└{line}┘");
        }

        public static void WriteCentered(string text,RenderOptions options)
        {
            text ??= "";
            int width = Math.Max(10, Math.Min(Console.WindowWidth - 2, 72));
            int pad = Math.Max(0, (width - text.Length) / 2);
            ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine(" " + new string(' ', pad) + text));
        }

        public static void Warn(string message, RenderOptions options)
        {
            ThemedConsole.With(options.Theme, options.Theme.Warn, () => Console.WriteLine(message));
        }

        public static void Prompt(string message, RenderOptions options)
        {
            ThemedConsole.With(options.Theme, options.Theme.Type, () => Console.Write(message));
            Console.ReadLine();
        }

        public static void WriteError(string msg, RenderOptions options)
        {
            ThemedConsole.With(options.Theme, options.Theme.Error, () => Console.WriteLine(msg));
        }

        public static void WriteInfo(string msg, RenderOptions options)
        {
            ThemedConsole.With(options.Theme, options.Theme.Info, () => Console.WriteLine(msg));
        }
    }
}
