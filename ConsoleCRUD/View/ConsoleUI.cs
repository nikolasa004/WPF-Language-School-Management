namespace ConsoleCRUD.View
{
    //Contains helper methods for console UI rendering.
    internal static class ConsoleUI
    {
        public static void ClearWithHeader(string title)
        {
            Console.Clear();
            var line = new string('─', Math.Max(10, Math.Min(Console.WindowWidth - 4, 72)));
            Console.WriteLine($"┌{line}┐");
            WriteCentered(title);
            Console.WriteLine($"└{line}┘");
        }

        public static void WriteCentered(string text)
        {
            text ??= "";
            int width = Math.Max(10, Math.Min(Console.WindowWidth - 2, 72));
            int pad = Math.Max(0, (width - text.Length) / 2);
            Console.WriteLine(" " + new string(' ', pad) + text);
        }

        public static void Warn(string message)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = old;
        }

        public static void Prompt(string message)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(message);
            Console.ForegroundColor = old;
            Console.ReadLine();
        }

        public static void WriteError(string msg)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = old;
        }

        public static void WriteInfo(string msg)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(msg);
            Console.ForegroundColor = old;
        }
    }
}
