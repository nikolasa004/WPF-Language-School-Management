namespace ConsoleCRUD.View.Styling
{   
    // BlackRed
    public sealed class BlackRedTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkRed;
        public ConsoleColor CommandKey => ConsoleColor.Red;
        public ConsoleColor Header => ConsoleColor.Red;
        public ConsoleColor Name => ConsoleColor.DarkRed;
        public ConsoleColor Type => ConsoleColor.Red;
        public ConsoleColor Value => ConsoleColor.DarkRed;
        public ConsoleColor Info => ConsoleColor.DarkGray;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // GreenBlack
    public sealed class HackerTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkGreen;
        public ConsoleColor CommandKey => ConsoleColor.Green;
        public ConsoleColor Header => ConsoleColor.Green;
        public ConsoleColor Name => ConsoleColor.Green;
        public ConsoleColor Type => ConsoleColor.DarkGreen;
        public ConsoleColor Value => ConsoleColor.Gray;
        public ConsoleColor Info => ConsoleColor.DarkGray;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.DarkRed;
    }

    // Candy
    public sealed class CandyTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.Magenta;
        public ConsoleColor CommandKey => ConsoleColor.Yellow;
        public ConsoleColor Header => ConsoleColor.Cyan;
        public ConsoleColor Name => ConsoleColor.Magenta;
        public ConsoleColor Type => ConsoleColor.Cyan;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.Blue;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // Standard
    public sealed class StandardTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkCyan;
        public ConsoleColor CommandKey => ConsoleColor.Yellow;
        public ConsoleColor Header => ConsoleColor.Gray;
        public ConsoleColor Name => ConsoleColor.Yellow;
        public ConsoleColor Type => ConsoleColor.Cyan;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkCyan;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // Minimal
    public sealed class MinimalTheme : ITheme
    {
        public ConsoleColor HelpBar => Console.ForegroundColor;
        public ConsoleColor CommandKey => Console.ForegroundColor;
        public ConsoleColor Header => Console.ForegroundColor;
        public ConsoleColor Name => Console.ForegroundColor;
        public ConsoleColor Type => Console.ForegroundColor;
        public ConsoleColor Value => Console.ForegroundColor;
        public ConsoleColor Info => Console.ForegroundColor;
        public ConsoleColor Warn => Console.ForegroundColor;
        public ConsoleColor Error => Console.ForegroundColor;
    }

    // DarkMode
    public sealed class DarkModeTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkGray;
        public ConsoleColor CommandKey => ConsoleColor.Gray;
        public ConsoleColor Header => ConsoleColor.White;
        public ConsoleColor Name => ConsoleColor.Gray;
        public ConsoleColor Type => ConsoleColor.DarkGray;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkGray;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // BlueOcean
    public sealed class BlueOceanTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkBlue;
        public ConsoleColor CommandKey => ConsoleColor.Blue;
        public ConsoleColor Header => ConsoleColor.Cyan;
        public ConsoleColor Name => ConsoleColor.Blue;
        public ConsoleColor Type => ConsoleColor.DarkCyan;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkBlue;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // Blue-Yellow
    public sealed class BlueYellowTheme : ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkBlue;
        public ConsoleColor CommandKey => ConsoleColor.Yellow;
        public ConsoleColor Header => ConsoleColor.Cyan;
        public ConsoleColor Name => ConsoleColor.Yellow;
        public ConsoleColor Type => ConsoleColor.DarkCyan;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkBlue;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // Sunset
    public sealed class SunsetTheme: ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkYellow;
        public ConsoleColor CommandKey => ConsoleColor.Magenta;
        public ConsoleColor Header => ConsoleColor.Yellow;
        public ConsoleColor Name => ConsoleColor.Magenta;
        public ConsoleColor Type => ConsoleColor.DarkYellow;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkGray;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }

    // Forest
    public sealed class ForestTheme: ITheme
    {
        public ConsoleColor HelpBar => ConsoleColor.DarkGreen;
        public ConsoleColor CommandKey => ConsoleColor.Green;
        public ConsoleColor Header => ConsoleColor.Green;
        public ConsoleColor Name => ConsoleColor.Green;
        public ConsoleColor Type => ConsoleColor.DarkGreen;
        public ConsoleColor Value => ConsoleColor.White;
        public ConsoleColor Info => ConsoleColor.DarkGray;
        public ConsoleColor Warn => ConsoleColor.Yellow;
        public ConsoleColor Error => ConsoleColor.Red;
    }
}
