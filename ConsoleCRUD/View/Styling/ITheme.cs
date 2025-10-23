namespace ConsoleCRUD.View.Styling
{
    public interface ITheme
    {
        ConsoleColor HelpBar { get; }
        ConsoleColor CommandKey { get; }
        ConsoleColor Header { get; }
        ConsoleColor Name { get; }
        ConsoleColor Type { get; }
        ConsoleColor Value { get; }
        ConsoleColor Info { get; }
        ConsoleColor Warn { get; }
        ConsoleColor Error { get; }
    }
}
