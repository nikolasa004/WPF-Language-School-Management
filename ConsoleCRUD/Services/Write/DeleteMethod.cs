using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View;
using ConsoleCRUD.View.Read;
using ConsoleCRUD.View.Styling;
using System.Collections;
using System.Globalization;
using System.Reflection;
namespace ConsoleCRUD.Services.Write{
    internal static class DeleteMethod{
        public static void DoDelete(Type modelType, object daoInstance, RenderOptions options){
            var getAll = DaoReadResolver.ResolveGetAll(daoInstance.GetType());
            if (getAll == null) { ConsoleUI.WriteError("GetAll metod nije pronađen.", options); return; }
            var list = (getAll.Invoke(daoInstance, null) as IEnumerable)?.Cast<object>().ToList() ?? new();
            if (list.Count == 0) { ConsoleUI.WriteInfo("(nema zapisa)", options); return; }
            ConsoleTableRenderer.Render(modelType, list, options: options);
            var keyProp = DaoWriteResolver.ResolveKeyProperty(modelType);
            if (TryDeleteByKeyFlow(modelType, daoInstance, options, keyProp)) return;
            if (FallbackDeleteByEntity(modelType, daoInstance, options, list)) return;
            if (FallbackDeleteByIndexAndKey(modelType, daoInstance, options, list, keyProp)) return;
            ConsoleUI.WriteError("Nisam našao odgovarajući Delete/Remove metod u DAO (ni po ključu ni po entitetu).", options);
        }
        private static bool TryDeleteByKeyFlow(Type modelType, object dao, RenderOptions opt, PropertyInfo? keyProp){
            if (keyProp == null) return false;
            var keyName = keyProp.Name;
            var keyType = Nullable.GetUnderlyingType(keyProp.PropertyType) ?? keyProp.PropertyType;
            ConsoleUI.Write($"Unesi {keyName} za brisanje (ENTER za izbor po indeksu): ", opt);
            var raw = (Console.ReadLine() ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return false;
            if (!TryParseKey(keyType, raw, out var keyVal)){
                ConsoleUI.WriteError($"Nevažeća vrednost za {keyName}.", opt);
                return true;
            }
            var delByKey = DaoWriteResolver.ResolveDeleteByKey(dao.GetType(), keyName, keyType, modelType.Name);
            if (delByKey == null) { ConsoleUI.WriteWarn($"Nema DAO metode za brisanje po {keyName}.", opt); return false; }
            if (!ConsoleFormHelpers.Confirm($"Da li si siguran da želiš da obrišeš {modelType.Name} sa {keyName} = {raw}? (y/N): ", opt))
                return true;
            return InvokeDeleteThenPersist(dao, delByKey, new[] { keyVal }, opt);
        }
        private static bool FallbackDeleteByEntity(Type modelType, object dao, RenderOptions opt, List<object> list){
            var delEntity = DaoWriteResolver.ResolveDelete(dao.GetType(), modelType);
            if (delEntity == null) return false;

            var idx = ConsoleFormHelpers.ReadIndex($"Index za brisanje (0..{list.Count - 1}): ", list.Count, opt);
            if (idx < 0) return true;

            var entity = list[idx];
            if (!ConsoleFormHelpers.Confirm("Da li si siguran? (y/N): ", opt)) return true;

            return InvokeDeleteThenPersist(dao, delEntity, new[] { entity }, opt);
        }
        private static bool FallbackDeleteByIndexAndKey(Type modelType, object dao, RenderOptions opt, List<object> list, PropertyInfo? keyProp){
            if (keyProp == null) return false;
            var delByKey = DaoWriteResolver.ResolveDeleteByKey(
                dao.GetType(),
                keyProp.Name,
                Nullable.GetUnderlyingType(keyProp.PropertyType) ?? keyProp.PropertyType,
                modelType.Name);
            if (delByKey == null) return false;
            var idx = ConsoleFormHelpers.ReadIndex($"Index za brisanje (0..{list.Count - 1}): ", list.Count, opt);
            if (idx < 0) return true;
            var entity = list[idx];
            var keyVal = keyProp.GetValue(entity);
            if (!ConsoleFormHelpers.Confirm("Da li si siguran? (y/N): ", opt)) return true;
            return InvokeDeleteThenPersist(dao, delByKey, new[] { keyVal }, opt);
        }
        private static bool InvokeDeleteThenPersist(object dao, MethodInfo deleteMi, object?[] args, RenderOptions opt){
            try{
                deleteMi.Invoke(dao, args);
                PersistIfPossible(dao, opt);
                ConsoleUI.WriteSuccess("Obrisano.", opt);
                return true;
            }
            catch (Exception ex){
                ConsoleUI.WriteError($"Greška pri brisanju: {ex.Message}", opt);
                return true;
            }
        }
        private static void PersistIfPossible(object daoInstance, RenderOptions options){
            var save = DaoWriteResolver.ResolveSave(daoInstance.GetType());
            if (save == null)
            {
                ConsoleUI.WriteWarn("Upozorenje: DAO nema Save/SaveChanges – promene možda nisu upisane u JSON.", options);
                return;
            }
            try { save.Invoke(daoInstance, null); }
            catch { ConsoleUI.WriteWarn("Upozorenje: upis u JSON nije uspeo.", options); }
        }
        private static bool TryParseKey(Type keyType, string raw, out object? value){
            value = null;
            keyType = Nullable.GetUnderlyingType(keyType) ?? keyType;
            if (keyType == typeof(string)) { value = raw; return true; }
            if (keyType == typeof(int)) { if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (keyType == typeof(long)) { if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (keyType == typeof(Guid)) { if (Guid.TryParse(raw, out var v)) { value = v; return true; } }
            return false;
        }}}