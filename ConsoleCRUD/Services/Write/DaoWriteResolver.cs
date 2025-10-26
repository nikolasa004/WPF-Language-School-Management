using System.Reflection;

namespace ConsoleCRUD.Services.Write
{
    internal static class DaoWriteResolver
    {
        public static MethodInfo? ResolveAdd(Type daoType, Type modelType)
            => FindMethodByEntity(daoType, modelType, new[] { "Add", "Insert", "Create" });
        public static MethodInfo? ResolveUpdate(Type daoType, Type modelType)
            => FindMethodByEntity(daoType, modelType, new[] { "Update", "Edit", "Save" });
        public static MethodInfo? ResolveDelete(Type daoType, Type modelType)
            => FindMethodByEntity(daoType, modelType, new[] { "Delete", "Remove" });
        public static MethodInfo? ResolveDeleteByKey(Type daoType, string keyName, Type keyType, string modelName)
        {
            keyType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            var nameCandidates = new[]
            {
                $"DeleteBy{keyName}",
                $"RemoveBy{keyName}",
                $"Delete{modelName}By{keyName}",
                $"Remove{modelName}By{keyName}"
            };
            foreach (var m in daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!nameCandidates.Any(n => m.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var pars = m.GetParameters();
                if (pars.Length == 1 &&
                    (Nullable.GetUnderlyingType(pars[0].ParameterType) ?? pars[0].ParameterType) == keyType)
                    return m;
            }
            foreach (var m in daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!(m.Name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) ||
                      m.Name.StartsWith("Remove", StringComparison.OrdinalIgnoreCase)))
                    continue;
                var pars = m.GetParameters();
                if (pars.Length == 1 &&
                    (Nullable.GetUnderlyingType(pars[0].ParameterType) ?? pars[0].ParameterType) == keyType)
                {
                    return m;
                }
            }

            return null;
        }
        public static PropertyInfo? ResolveKeyProperty(Type modelType)
        {
            var p = modelType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                 ?? modelType.GetProperty("ID", BindingFlags.Public | BindingFlags.Instance);
            if (p != null) return p;
            p = modelType.GetProperty("Email", BindingFlags.Public | BindingFlags.Instance);
            if (p != null) return p;
            return null;
        }
        private static MethodInfo? FindMethodByEntity(Type daoType, Type modelType, string[] prefixes)
        {
            foreach (var m in daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prefixes.Any(p => m.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var pars = m.GetParameters();
                if (pars.Length == 1 && pars[0].ParameterType.IsAssignableFrom(modelType))
                    return m;
            }
            return null;
        }
        public static MethodInfo? ResolveSave(Type daoType)
            => daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name.Equals("Save", StringComparison.OrdinalIgnoreCase) ||
                                           m.Name.Equals("SaveChanges", StringComparison.OrdinalIgnoreCase));
        public static MethodInfo? ResolveNextId(Type daoType)
            => daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name.Equals("NextId", StringComparison.OrdinalIgnoreCase) ||
                                           m.Name.Equals("GetNextId", StringComparison.OrdinalIgnoreCase));
    }
}
