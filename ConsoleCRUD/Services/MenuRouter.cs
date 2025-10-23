using ConsoleCRUD.View;

namespace ConsoleCRUD.Services
{
    public enum CrudAction { Add, List, Edit, Delete }

    //Contains the main loop for the console CRUD application, calls entity selection, CRUD action selection, and execution.
    internal sealed class MenuRouter
    {
        private readonly IReadOnlyDictionary<string, Type> _catalog;
        private readonly MenuView _menu;
        private readonly ExecutorCRUD _executor;

        public MenuRouter(IReadOnlyDictionary<string, Type> catalog, IReadOnlyDictionary<Type, object> daoCatalog)
        {
            _catalog = catalog;
            _menu = new MenuView();
            _executor= new ExecutorCRUD(catalog, daoCatalog);
        }

        public void Run()
        {
            while (true)
            {
                var (exitRequested, chosenKey, chosenType) = _menu.ShowEntityMenu(_catalog);
                if (exitRequested) break;
                if (chosenType == null) continue;

                var (goBack, crudAction) = _menu.ShowCrudMenu(chosenKey!);
                if (goBack) continue;

                _executor.Execute(crudAction, chosenKey!);
            }
        }
    }
}
