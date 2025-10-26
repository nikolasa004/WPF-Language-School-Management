using System;
using System.Collections.Generic;

namespace ConsoleCRUD.Services.Write
{
    internal static class PromptRules
    {
        private static readonly Dictionary<Type, Dictionary<string, Func<object, object?>>> _rules =
            new Dictionary<Type, Dictionary<string, Func<object, object?>>>();

        public static void Skip<T>(params string[] propertyNames) where T : class
        {
            foreach (var name in propertyNames)
                RegisterInternal(typeof(T), name, provider: null);
        }

        public static void SkipWithDefault<T>(string propertyName, object? constantValue) where T : class
        {
            RegisterInternal(typeof(T), propertyName, _ => constantValue);
        }

        public static void SkipWithDefault<T>(string propertyName, Func<T, object?> provider) where T : class
        {
            RegisterInternal(typeof(T), propertyName, e => provider((T)e));
        }

        public static bool TryGetSkipDefault(Type entityType, string propertyName, out Func<object, object?>? providerOrNull)
        {
            providerOrNull = null;
            if (!_rules.TryGetValue(entityType, out var map)) return false;
            if (!map.TryGetValue(propertyName, out var provider)) return false;
            providerOrNull = provider ?? (_ => null);
            return true;
        }

        private static void RegisterInternal(Type entityType, string propertyName, Func<object, object?>? provider)
        {
            if (!_rules.TryGetValue(entityType, out var map))
            {
                map = new Dictionary<string, Func<object, object?>>(StringComparer.OrdinalIgnoreCase);
                _rules[entityType] = map;
            }
            map[propertyName] = provider ?? (_ => null);
        }
    }
}
