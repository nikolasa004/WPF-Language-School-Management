using ConsoleCRUD.Services;
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

            var view = new MenuRouter(typesCatalog, daoCatalog);
            view.Run();
        }
    }
}
