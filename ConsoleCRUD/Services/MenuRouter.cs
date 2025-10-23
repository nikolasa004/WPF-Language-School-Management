using ConsoleCRUD.View;
using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.Services
{
    public enum CrudAction { Add, List, Edit, Delete }

    //Contains the main loop for the console CRUD application, calls entity selection, CRUD action selection, and execution.
    internal sealed class MenuRouter
    {
        private readonly IReadOnlyDictionary<string, Type> _catalog;
        private readonly MenuView _menu;
        private readonly ExecutorCRUD _executor;
        private readonly RenderOptions option;

        public MenuRouter(IReadOnlyDictionary<string, Type> catalog, IReadOnlyDictionary<Type, object> daoCatalog, RenderOptions option)
        {
            _catalog = catalog;
            _menu = new MenuView();
            this.option = option;
            _executor = new ExecutorCRUD(catalog, daoCatalog,option);
        }

        public void Run()
        {
            while (true)
            {
                var (exitRequested, chosenKey, chosenType) = _menu.ShowEntityMenu(_catalog,option);
                if (exitRequested) break;
                if (chosenType == null) continue;

                var (goBack, crudAction) = _menu.ShowCrudMenu(chosenKey!,option);
                if (goBack) continue;

                _executor.Execute(crudAction, chosenKey!);
            }
        }
    }
}
