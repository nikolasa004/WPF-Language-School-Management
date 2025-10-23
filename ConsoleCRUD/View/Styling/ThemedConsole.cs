namespace ConsoleCRUD.View.Styling
{
    internal static class ThemedConsole
    {
        public static void With(ITheme theme, ConsoleColor color, Action act)
        {
            var old = Console.ForegroundColor;
            try { Console.ForegroundColor = color; act(); }
            finally { Console.ForegroundColor = old; }
        }
    }
}
