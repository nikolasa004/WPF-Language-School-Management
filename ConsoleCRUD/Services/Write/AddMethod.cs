using ConsoleCRUD.View;
using ConsoleCRUD.View.Styling;
using System.Globalization;
using System.Reflection;
namespace ConsoleCRUD.Services.Write
{
    internal static class AddMethod
    {
        public static void DoAdd(Type modelType, object daoInstance, RenderOptions options){
            var entity = CreateEntity(modelType, options);
            if (entity == null) return;
            RenderAddHeader(modelType, options);
            ConsoleFormHelpers.FillObjectFromConsole(entity, keepExistingOnEmpty: false, options);
            EnsureIdIfNeeded(modelType, daoInstance, entity);
            var add = DaoWriteResolver.ResolveAdd(daoInstance.GetType(), modelType);
            if (add == null) { ConsoleUI.WriteError("Nisam našao Add/Insert/Create metod u DAO.", options); return; }
            InvokeAdd(add, daoInstance, entity, options);
            PersistIfPossible(daoInstance, options);
            ConsoleUI.WriteSuccess("Uspešno dodato.", options);
        }
        private static object? CreateEntity(Type modelType, RenderOptions options){
            var entity = Activator.CreateInstance(modelType);
            if (entity == null)
                ConsoleUI.WriteError($"Ne mogu da kreiram instancu: {modelType.Name}", options);
            return entity;
        }
        private static void RenderAddHeader(Type modelType, RenderOptions options)
            => ConsoleUI.WriteHeader($"ADD — {modelType.Name}", options);
        private static void EnsureIdIfNeeded(Type modelType, object daoInstance, object entity){
            var idProp = modelType.GetProperty("Id") ?? modelType.GetProperty("ID");
            var nextIdMi = DaoWriteResolver.ResolveNextId(daoInstance.GetType());
            if (idProp == null || nextIdMi == null) return;
            var current = idProp.GetValue(entity);
            if (!IsDefaultNumeric(current, idProp.PropertyType)) return;
            TryAssignNextId(idProp, nextIdMi, daoInstance, entity);
        }
        private static bool IsDefaultNumeric(object? value, Type propType){
            var t = Nullable.GetUnderlyingType(propType) ?? propType;
            if (value == null) return true;
            return t == typeof(int) && (int)value == 0
                || t == typeof(long) && (long)value == 0L
                || t == typeof(short) && (short)value == 0
                || t == typeof(uint) && (uint)value == 0u
                || t == typeof(ulong) && (ulong)value == 0UL;
        }
        private static void TryAssignNextId(PropertyInfo idProp, MethodInfo nextIdMi, object daoInstance, object entity){
            try
            {
                var next = nextIdMi.Invoke(daoInstance, null);
                var targetType = Nullable.GetUnderlyingType(idProp.PropertyType) ?? idProp.PropertyType;
                var converted = Convert.ChangeType(next, targetType, CultureInfo.InvariantCulture);
                idProp.SetValue(entity, converted);
            }
            catch { }
        }
        private static void InvokeAdd(MethodInfo add, object daoInstance, object entity, RenderOptions options){
            try { add.Invoke(daoInstance, new[] { entity }); }
            catch (Exception ex) { ConsoleUI.WriteError($"Greška pri dodavanju: {ex.Message}", options); throw; }
        }
        private static void PersistIfPossible(object daoInstance, RenderOptions options){
            var save = DaoWriteResolver.ResolveSave(daoInstance.GetType());
            if (save == null)
            {
                ConsoleUI.WriteWarn("Upozorenje: DAO nema Save/SaveChanges metod – promene možda nisu upisane u JSON.", options);
                return;
            }
            try { save.Invoke(daoInstance, null); }
            catch { ConsoleUI.WriteWarn("Upozorenje: upis u JSON nije uspeo.", options); }
        }}}
