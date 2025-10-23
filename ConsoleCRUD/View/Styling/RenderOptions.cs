namespace ConsoleCRUD.View.Styling
{
    public class RenderOptions
    {
        public ITheme Theme { get; init; } = new StandardTheme();
        public ITableSkin Skin { get; init; } = new AsciiSkin();

        public static RenderOptions Default() => new RenderOptions();
    }
}
