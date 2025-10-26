using ConsoleCRUD.Services;
using ConsoleCRUD.Services.Read;
using ConsoleCRUD.Services.Write;
using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.View
{
    internal class ExecutorCRUD
    {
        private readonly IReadOnlyDictionary<string, Type> _typeCatalog;
        private readonly IReadOnlyDictionary<Type, object> _daoMap;
        private readonly RenderOptions option;

        public ExecutorCRUD(IReadOnlyDictionary<string, Type> typeCatalog,IReadOnlyDictionary<Type, object> daoMap, RenderOptions option)
        {
            _typeCatalog = typeCatalog;
            _daoMap = daoMap;
            this.option = option;
        }

        public void Execute(CrudAction operation, string displayKey)
        {
            if (!_typeCatalog.TryGetValue(displayKey, out var modelType)){
                ConsoleUI.WriteError($"Ne poznajem entitet '{displayKey}'. (Nije u katalogu tipova)", option);
                return;
            }

            if (!_daoMap.TryGetValue(modelType, out var daoInstance)){
                ConsoleUI.WriteError($"Nema mapiranog DAO za tip {modelType.FullName}. (Bootstrap ga nije našao)", option);
                return;
            }

            switch (operation){
                case CrudAction.List:
                    GetAllMethod.DoList(modelType, daoInstance, option);
                    break;

                case CrudAction.Add:
                    AddMethod.DoAdd(modelType, daoInstance, option);
                    break;

                case CrudAction.Edit:
                    EditMethod.DoEdit(modelType, daoInstance, option);
                    break;

                case CrudAction.Delete:
                    DeleteMethod.DoDelete(modelType, daoInstance, option);
                    break;

                default:
                    ConsoleUI.WriteError("Nepoznata CRUD akcija.", option);
                    break;
            }
        }
    }
}
