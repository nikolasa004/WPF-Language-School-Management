using ConsoleCRUD.View;
using ConsoleCRUD.View.Read;
using ConsoleCRUD.View.Styling;
using System.Collections;
using System.Reflection;

namespace ConsoleCRUD.Services.Read
{
    //Executes the 'GetAll' method on the appropriate DAO and displays the results in a table.
    internal class GetAllMethod
    {
        public static void DoList(Type modelType, object daoInstance, RenderOptions option)
        {
            var methodInfo = ResolveGetAllMethod(daoInstance.GetType(), modelType);
            if (methodInfo == null)
            {
                ConsoleUI.WriteError($"Nisam našao odgovarajući 'GetAll' metod u DAO: {daoInstance.GetType().FullName}", option);
                return;
            }

            object? result;
            try
            {
                result = methodInfo.Invoke(daoInstance, null);
            }
            catch (Exception ex)
            {
                ConsoleUI.WriteError($"Poziv {methodInfo.Name} nije uspeo: {ex.Message}", option);
                return;
            }

            if (result is not IEnumerable enumerable)
            {
                ConsoleUI.WriteError($"Metod {methodInfo.Name} ne vraća IEnumerable.", option);
                return;
            }


            var genericEnumerable = enumerable.Cast<object>();

            ConsoleTableRenderer.Render(modelType, genericEnumerable, options:option);
        }

        private static MethodInfo? ResolveGetAllMethod(Type daoType, Type modelType)
        {
            var modelName = modelType.Name;
            var methodName = "GetAll";

            var methodInfo = daoType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, binder: null, types: Type.EmptyTypes, modifiers: null);

            if (typeof(IEnumerable).IsAssignableFrom(methodInfo!.ReturnType))
                return methodInfo;

            var any = daoType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.GetParameters().Length == 0 && typeof(IEnumerable).IsAssignableFrom(m.ReturnType));
            return any;
        }
    }
}
