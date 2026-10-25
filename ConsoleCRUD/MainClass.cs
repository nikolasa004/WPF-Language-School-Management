using ConsoleCRUD.Services;
using ConsoleCRUD.View.Styling;
using System.Reflection;

namespace ConsoleCRUD
{
    // Entry point of the ConsoleCRUD application
    internal class MainClass
    {
        static void Main(string[] args)
        {
            Dictionary<string, Type> typesCatalog=TypeGetter.BuildTypeCatalog(Assembly.Load("LangLang"));
            Dictionary<Type,object> daoCatalog = DaoResolver.MapAllDaos(typesCatalog);

            //Standard + ASCII
            RenderOptions opt1 = new RenderOptions { Skin = new AsciiSkin(), Theme = new StandardTheme() };
            //BlackRed + ASCII
            RenderOptions opt2 = new RenderOptions { Skin = new AsciiSkin(), Theme = new BlackRedTheme() };
            //Hacker + ASCII
            RenderOptions opt3 = new RenderOptions { Skin = new AsciiSkin(), Theme = new HackerTheme() };
            //Candy + ASCII
            RenderOptions opt4 = new RenderOptions { Skin = new AsciiSkin(), Theme = new CandyTheme() };
            //Minimal + ASCII
            RenderOptions opt5 = new RenderOptions { Skin = new AsciiSkin(), Theme = new MinimalTheme() };
            //Standard + Minimal
            RenderOptions opt6 = new RenderOptions { Skin = new MinimalSkin(), Theme = new StandardTheme() };
            //Forest + Dollar
            RenderOptions opt7 = new RenderOptions { Skin = new DollarSkin(), Theme = new ForestTheme() };
            //Dark + Hash
            RenderOptions opt8 = new RenderOptions { Skin = new HashStarSkin(), Theme = new DarkModeTheme() };
            //BlueYellow + Exclamation
            RenderOptions opt9 = new RenderOptions { Skin = new ExclaimEqualSkin(), Theme = new BlueYellowTheme() };
            //BlueOcean + TildeWave
            RenderOptions opt10 = new RenderOptions { Skin = new TildeWaveSkin(), Theme = new BlueOceanTheme() };
            //Sunset + EightZen
            RenderOptions opt11 = new RenderOptions { Skin = new ZenEightSkin(), Theme = new SunsetTheme() };

            var view = new MenuRouter(typesCatalog, daoCatalog, opt7);
            view.Run();
        }
    }
}
