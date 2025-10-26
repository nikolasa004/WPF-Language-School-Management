using System.Reflection;

namespace ConsoleCRUD.Services.Write
{
    internal static class TypeInspector
    {
        public static bool IsPrimitiveLike(Type t)
        {
            if (t.IsEnum) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(bool) || t == typeof(char)) return true;

            if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(short) || t == typeof(ushort) ||
                t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong)) return true;

            if (t == typeof(float) || t == typeof(double) || t == typeof(decimal)) return true;

            if (t == typeof(DateTime)) return true;
            if (t == typeof(Guid)) return true;

            return false;
        }

        public static bool IsListOfPrimitiveLike(Type t, out Type? elementType)
        {
            elementType = null;
            if (!t.IsGenericType) return false;
            if (t.GetGenericTypeDefinition() != typeof(List<>)) return false;

            var elem = t.GetGenericArguments()[0];
            if (!IsPrimitiveLike(elem)) return false;

            elementType = elem;
            return true;
        }

        public static bool IsListOfTupleOfEnums(Type t, out Type? enumA, out Type? enumB)
        {
            enumA = null; enumB = null;
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(List<>)) return false;

            var inner = t.GetGenericArguments()[0];
            if (!inner.IsGenericType || inner.GetGenericTypeDefinition() != typeof(Tuple<,>)) return false;

            var args = inner.GetGenericArguments();
            if (args.Length != 2) return false;

            if (args[0].IsEnum && args[1].IsEnum)
            {
                enumA = args[0];
                enumB = args[1];
                return true;
            }
            return false;
        }

        public static bool IsDictionaryOfEnums(Type t, out Type? keyEnum, out Type? valEnum)
        {
            keyEnum = null; valEnum = null;
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>)) return false;

            var args = t.GetGenericArguments();
            if (args.Length != 2) return false;

            if (args[0].IsEnum && args[1].IsEnum)
            {
                keyEnum = args[0];
                valEnum = args[1];
                return true;
            }
            return false;
        }
    }
}
