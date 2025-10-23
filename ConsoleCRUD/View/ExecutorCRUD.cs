using ConsoleCRUD.Services;
using ConsoleCRUD.Services.Read;

namespace ConsoleCRUD.View
{
    //Executes chosen CRUD operation on selected entity type.
    internal class ExecutorCRUD
    {
        private readonly IReadOnlyDictionary<string, Type> _typeCatalog;
        private readonly IReadOnlyDictionary<Type, object> _daoMap;

        public ExecutorCRUD(IReadOnlyDictionary<string, Type> typeCatalog,IReadOnlyDictionary<Type, object> daoMap)
        {
            _typeCatalog = typeCatalog;
            _daoMap = daoMap;
        }

        public void Execute(CrudAction operation, string displayKey)
        {
            if (!_typeCatalog.TryGetValue(displayKey, out var modelType))
            {
                ConsoleUI.WriteError($"Ne poznajem entitet '{displayKey}'. (Nije u katalogu tipova)");
                return;
            }

            if (!_daoMap.TryGetValue(modelType, out var daoInstance))
            {
                ConsoleUI.WriteError($"Nema mapiranog DAO za tip {modelType.FullName}. (Bootstrap ga nije našao)");
                return;
            }

            switch (operation)
            {
                case CrudAction.List:
                    GetAllMethod.DoList(modelType, daoInstance);
                    break;

                case CrudAction.Add:
                    ConsoleUI.WriteInfo("ADD: (stub) Ovde će ići kreiranje entiteta.");
                    break;

                case CrudAction.Edit:
                    ConsoleUI.WriteInfo("EDIT: (stub) Ovde će ići ažuriranje entiteta.");
                    break;

                case CrudAction.Delete:
                    ConsoleUI.WriteInfo("DELETE: (stub) Ovde će ići brisanje entiteta.");
                    break;

                default:
                    ConsoleUI.WriteError("Nepoznata CRUD akcija.");
                    break;
            }
        }
    }
}
