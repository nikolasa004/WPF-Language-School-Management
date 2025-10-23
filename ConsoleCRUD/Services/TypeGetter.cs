using System.Reflection;

namespace ConsoleCRUD.Services
{
    //Gets all relevant types from the LangLang.Model namespace to build a type catalog
    internal class TypeGetter
    {
        private const string ModelsNamespacePrefix = "LangLang.Model";
        private static readonly string[] ExcludedNamespaceSuffixes = { ".DAO", ".Enums" };

        public static Dictionary<string, Type> BuildTypeCatalog(Assembly assembly)
        {
            var types = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.IsPublic &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith(ModelsNamespacePrefix, StringComparison.Ordinal) &&
                    !ExcludedNamespaceSuffixes.Any(ex => t.Namespace.EndsWith(ex, StringComparison.Ordinal)) &&
                    !t.IsGenericTypeDefinition && 
                    !t.IsNested)
                .ToList();

            var typeCatalog = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in types)
            {
                var key = t.Name;
                if (typeCatalog.ContainsKey(key))
                {
                    key = t.FullName!;
                }
                typeCatalog[key] = t;
            }

            return typeCatalog;
        }
    }
}
