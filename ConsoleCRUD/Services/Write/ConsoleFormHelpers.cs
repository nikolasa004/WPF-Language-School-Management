using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View;
using ConsoleCRUD.View.Styling;
using System.Collections;
using System.Reflection;

namespace ConsoleCRUD.Services.Write{
    internal static class ConsoleFormHelpers{
        public static void FillObjectFromConsole(object entity, bool keepExistingOnEmpty, RenderOptions options){
            var props = entity.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);
            foreach (var p in props){
                if (PromptRules.TryGetSkipDefault(entity.GetType(), p.Name, out var defaultProvider)){
                    TrySetDefault(entity, p, defaultProvider);
                    continue;
                }
                var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                if (TypeInspector.IsDictionaryOfEnums(t, out var keyEnum, out var valEnum)){
                    PromptDictionaryOfEnums(entity, p, keyEnum!, valEnum!, keepExistingOnEmpty, options);
                    continue;
                }
                if (TypeInspector.IsListOfTupleOfEnums(t, out var enumA, out var enumB)){
                    PromptTupleEnumList(entity, p, enumA!, enumB!, keepExistingOnEmpty, options);
                    continue;
                }
                if (TypeInspector.IsListOfPrimitiveLike(t, out var elemType)){
                    PromptPrimitiveList(entity, p, elemType!, keepExistingOnEmpty, options);
                    continue;
                }
                if (TypeInspector.IsPrimitiveLike(t)){
                    PromptPrimitive(entity, p, t, keepExistingOnEmpty, options);
                    continue;
                }
                ConsoleUI.WriteWarn($"{p.Name} [{t.Name}] preskočeno (složeni tip).", options);
            }
        }
        public static int ReadIndex(string prompt, int count, RenderOptions options){
            ConsoleUI.Write(prompt, options);
            if (!int.TryParse(Console.ReadLine(), out var idx)) return -1;
            if (idx < 0 || idx >= count) return -1;
            return idx;
        }
        public static bool Confirm(string prompt, RenderOptions options){
            ConsoleUI.Write(prompt, options);
            var s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            return s is "y" or "yes" or "da" or "d";
        }
        private static void TrySetDefault(object entity, PropertyInfo p, Func<object, object?>? provider){
            try { var def = provider?.Invoke(entity); if (def != null) p.SetValue(entity, def); }
            catch { /* intentionally ignore */ }
        }
        private static void PromptPrimitive(object entity, PropertyInfo p, Type t, bool keepExistingOnEmpty, RenderOptions options){
            while (true){
                var current = p.GetValue(entity);
                var hint = current == null ? "" : $" (current: {TypeValueHelpers.FormatPrimitive(current)})";
                ConsoleUI.Write($"{p.Name} [{FormatHelpers.PrettyType(t)}]{hint}: ", options);
                var input = Console.ReadLine();
                if (keepExistingOnEmpty && string.IsNullOrWhiteSpace(input)) break;
                if (PrimitiveParser.TryParse(t, input, out var parsed)){
                    p.SetValue(entity, parsed);
                    break;
                }
                ConsoleUI.WriteWarn("Nevažeća vrednost. Pokušaj ponovo.", options);
            }}
        private static void PromptPrimitiveList(object entity, PropertyInfo p, Type elemType, bool keepExistingOnEmpty, RenderOptions options){
            while (true){
                var current = p.GetValue(entity);
                var hint = current == null ? "" : $" (current: {FormatHelpers.FormatList(current as IEnumerable)})";
                ConsoleUI.Write($"{p.Name} [List<{elemType.Name}>] (npr. 1,2,3 ili [1, 2, 3]){hint}: ", options);
                var input = Console.ReadLine();
                if (keepExistingOnEmpty && string.IsNullOrWhiteSpace(input)) break;
                if (PrimitiveListParser.TryParse(elemType, input ?? "", out var parsedList)){
                    p.SetValue(entity, parsedList);
                    break;
                }
                ConsoleUI.WriteWarn("Nevažeća lista. Pokušaj ponovo.", options);
            }}
        private static void PromptTupleEnumList(object entity, PropertyInfo p, Type enumA, Type enumB, bool keepExistingOnEmpty, RenderOptions options){
            while (true){
                var current = p.GetValue(entity);
                var hint = current == null ? "" : $" (current: {FormatHelpers.FormatTupleList(current as IEnumerable, enumA, enumB)})";
                var exA = FormatHelpers.GetEnumExamples(enumA, 2);
                var exB = FormatHelpers.GetEnumExamples(enumB, 2);
                var example = $"{exA[0]}:{exB[0]}, {exA[1]}:{exB[1]}";
                ConsoleUI.Write($"{p.Name} [List<Tuple<{enumA.Name},{enumB.Name}>>] (npr. {example} ili ({exA[0]},{exB[0]});({exA[1]},{exB[1]})){hint}: ", options);
                var input = Console.ReadLine();
                if (keepExistingOnEmpty && string.IsNullOrWhiteSpace(input)) break;
                if (TupleEnumListParser.TryParse(enumA, enumB, input ?? "", out var parsedList)){
                    p.SetValue(entity, parsedList);
                    break;
                }
                ConsoleUI.WriteWarn("Nevažeća lista tuple-ova. Pokušaj ponovo.", options);
            }}
        private static void PromptDictionaryOfEnums(object entity, PropertyInfo p, Type keyEnum, Type valEnum, bool keepExistingOnEmpty, RenderOptions options){
            while (true){
                var current = p.GetValue(entity);
                var hint = current == null ? "" : $" (current: {FormatHelpers.FormatEnumEnumDictionary(current as IEnumerable, keyEnum, valEnum)})";
                var exK = FormatHelpers.GetEnumExamples(keyEnum, 2);
                var exV = FormatHelpers.GetEnumExamples(valEnum, 2);
                var example = $"{exK[0]}:{exV[0]}, {exK[1]}:{exV[1]}";
                ConsoleUI.Write($"{p.Name} [Dictionary<{keyEnum.Name},{valEnum.Name}>] (npr. {example} ili ({exK[0]},{exV[0]});({exK[1]},{exV[1]})){hint}: ", options);
                var input = Console.ReadLine();
                if (keepExistingOnEmpty && string.IsNullOrWhiteSpace(input)) break;
                if (EnumEnumDictionaryParser.TryParse(keyEnum, valEnum, input ?? "", out var parsedDict)){
                    p.SetValue(entity, parsedDict);
                    break;
                }
                ConsoleUI.WriteWarn("Nevažeći rečnik (format). Pokušaj ponovo.", options);
            }}}}
