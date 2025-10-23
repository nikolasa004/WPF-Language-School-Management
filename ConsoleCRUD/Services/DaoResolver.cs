using System.Reflection;

namespace ConsoleCRUD.Services
{
    //Maps model types to their corresponding DAO instances
    internal class DaoResolver
    {
        public static Dictionary<Type, object> MapAllDaos(Dictionary<string, Type> modelCatalog)
        {
            var result = new Dictionary<Type, object>();

            foreach (var kv in modelCatalog.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var displayName = kv.Key;
                var modelType = kv.Value;

                var daoType = FindDaoType(modelType);
                if (daoType == null)
                {
                    modelCatalog.Remove(displayName);
                    continue;
                }

                object? instance = null;
                try
                {
                    instance = Activator.CreateInstance(daoType);
                }
                catch
                {
                    modelCatalog.Remove(displayName);
                    continue;
                }

                if (instance == null)
                {
                    modelCatalog.Remove(displayName);
                    continue;
                }

                result[modelType] = instance;
            }

            return result;
        }

        private static Type? FindDaoType(Type modelType)
        {
            var modelName = modelType.Name;

            string[] candidates =
            {
                modelName + "sDAO",
                modelName + "DAO",
            };

            string DaoAssemblyName = Assembly.Load("LangLang").FullName!.Split(',')[0];
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, DaoAssemblyName, StringComparison.OrdinalIgnoreCase))?? Assembly.Load(DaoAssemblyName);

            var matches = asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericTypeDefinition).Where(t => candidates.Contains(t.Name, StringComparer.OrdinalIgnoreCase)).ToList();

            return matches.Count == 1 ? matches[0] : null;
        }
    }
}
