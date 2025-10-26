using System.Collections;
using System.Reflection;

namespace ConsoleCRUD.Services.Read
{
    internal static class DaoReadResolver
    {
        public static MethodInfo? ResolveGetAll(Type daoType)
        {
            var m = daoType.GetMethod("GetAll",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase,
                binder: null, types: Type.EmptyTypes, modifiers: null);

            if (m != null && typeof(IEnumerable).IsAssignableFrom(m.ReturnType))
                return m;

            return daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          .FirstOrDefault(x => x.GetParameters().Length == 0 &&
                                               typeof(IEnumerable).IsAssignableFrom(x.ReturnType));
        }
    }
}
