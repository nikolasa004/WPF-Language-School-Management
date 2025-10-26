using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View;
using ConsoleCRUD.View.Read;
using ConsoleCRUD.View.Styling;
using System.Collections;
using System.Globalization;
using System.Reflection;
namespace ConsoleCRUD.Services.Write{
    internal static class EditMethod{
        public static void DoEdit(Type modelType, object daoInstance, RenderOptions options){
            var list = LoadAllOrReport(daoInstance, options);
            if (list == null || list.Count == 0) return;
            ConsoleTableRenderer.Render(modelType, list, options: options);
            var entity = SelectEntityByKeyOrIndex(modelType, list, options);
            if (entity == null) return;
            ConsoleUI.WriteHeader($"EDIT — {modelType.Name}", options);
            ConsoleFormHelpers.FillObjectFromConsole(entity, keepExistingOnEmpty: true, options);
            var update = DaoWriteResolver.ResolveUpdate(daoInstance.GetType(), modelType);
            if (update == null) { ConsoleUI.WriteError("Nisam našao Update/Edit/Save metod u DAO.", options); return; }
            try{
                update.Invoke(daoInstance, new[] { entity });
                PersistIfPossible(daoInstance, options);
                ConsoleUI.WriteSuccess("Uspešno izmenjeno.", options);  }
            catch (Exception ex){
                ConsoleUI.WriteError($"Greška pri izmeni: {ex.Message}", options);
            }}
        private static List<object>? LoadAllOrReport(object daoInstance, RenderOptions options){
            var getAll = DaoReadResolver.ResolveGetAll(daoInstance.GetType());
            if (getAll == null) { ConsoleUI.WriteError("GetAll metod nije pronađen.", options); return null; }
            var list = (getAll.Invoke(daoInstance, null) as IEnumerable)?.Cast<object>().ToList() ?? new();
            if (list.Count == 0) ConsoleUI.WriteInfo("(nema zapisa)", options);
            return list;}
        private static object? SelectEntityByKeyOrIndex(Type modelType, List<object> list, RenderOptions options){
            var keyProp = DaoWriteResolver.ResolveKeyProperty(modelType);
            if (keyProp != null && TryPromptAndFindByKey(list, keyProp, options, out var found)) return found;

            var idx = ConsoleFormHelpers.ReadIndex($"Index za izmenu (0..{list.Count - 1}): ", list.Count, options);
            return idx < 0 ? null : list[idx];}
        private static bool TryPromptAndFindByKey(List<object> list, PropertyInfo keyProp, RenderOptions options, out object? entity){
            entity = null;
            var keyName = keyProp.Name;
            var keyType = Nullable.GetUnderlyingType(keyProp.PropertyType) ?? keyProp.PropertyType;
            ConsoleUI.Write($"Unesi {keyName} za izmenu (ostavi prazno za izbor po indeksu): ", options);
            var raw = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return false;
            if (!TryParseKey(keyType, raw, out var keyVal)){
                ConsoleUI.WriteError($"Nevažeća vrednost za {keyName}.", options);
                return true;
            }
            entity = FindByKey(list, keyProp, keyVal);
            if (entity == null) ConsoleUI.WriteError($"Nije pronađen zapis sa {keyName} = {raw}.", options);
            return true;}
        private static void PersistIfPossible(object daoInstance, RenderOptions options){
            var save = DaoWriteResolver.ResolveSave(daoInstance.GetType());
            if (save == null){
                ConsoleUI.WriteWarn("Upozorenje: DAO nema Save/SaveChanges – promene možda nisu upisane u JSON.", options);
                return; }
            try { save.Invoke(daoInstance, null); }
            catch { ConsoleUI.WriteWarn("Upozorenje: upis u JSON nije uspeo.", options); }}
        private static bool TryParseKey(Type keyType, string raw, out object? value){
            value = null;
            keyType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            if (keyType == typeof(string)) { value = raw; return true; }
            if (keyType == typeof(int)) { if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (keyType == typeof(long)) { if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (keyType == typeof(Guid)) { if (Guid.TryParse(raw, out var v)) { value = v; return true; } }
            return false;}
        private static object? FindByKey(List<object> list, PropertyInfo keyProp, object keyVal){
            bool isString = (Nullable.GetUnderlyingType(keyProp.PropertyType) ?? keyProp.PropertyType) == typeof(string);
            foreach (var e in list){
                var v = keyProp.GetValue(e);
                if (isString){
                    var a = (v as string) ?? "";
                    var b = (keyVal as string) ?? "";
                    if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return e;
                }
                else if (Equals(v, keyVal)) return e;
            }
            return null;}}}
