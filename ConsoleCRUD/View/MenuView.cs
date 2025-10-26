using ConsoleCRUD.Services;
using ConsoleCRUD.View.Styling;

namespace ConsoleCRUD.View
{
    //Draws menus for entity selection and CRUD action selection.
    internal sealed class MenuView
    {
        public (bool exitRequested, string? chosenKey, Type? chosenType) ShowEntityMenu(IReadOnlyDictionary<string, Type> catalog, RenderOptions options)
        {
            ConsoleUI.ClearWithHeader("CONSOLE CRUD — Izbor entiteta",options);

            if (catalog.Count == 0){
                Console.WriteLine("Nema registrovanih modela za CRUD.");
                Console.WriteLine();
                ConsoleUI.Prompt("Pritisni Enter za izlaz...", options);
                return (true, null, null);
            }

            var keys = catalog.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

            ThemedConsole.With(options.Theme,options.Theme.Name,() => Console.WriteLine("Odaberi entitet:"));
            Console.WriteLine(new string('-', 60));
            for (int i = 0; i < keys.Count; i++){
                ThemedConsole.With(options.Theme, options.Theme.Value, () =>Console.WriteLine($"{i + 1,2}) {keys[i]}"));}
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("X) Izlaz iz aplikacije"); Console.WriteLine();

            while (true){
                Console.Write("Unos: ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input)) continue;

                if (input.Equals("x", StringComparison.OrdinalIgnoreCase)) return (true, null, null);

                if (int.TryParse(input, out int ix) && ix >= 1 && ix <= keys.Count){
                    var chosenKey = keys[ix - 1];
                    var chosenType = catalog[chosenKey];
                    return (false, chosenKey, chosenType);}

                ConsoleUI.Warn("Nevažeći izbor. Pokušaj ponovo (1..N ili X za izlaz).", options);
            }
        }

        public (bool goBack, CrudAction action) ShowCrudMenu(string chosenKey, RenderOptions options)
        {
            ConsoleUI.ClearWithHeader($"Entitet: {chosenKey} — Izbor operacije",options);

            ThemedConsole.With(options.Theme,options.Theme.Name,() => Console.WriteLine("Odaberi CRUD operaciju:"));
            Console.WriteLine(new string('-', 60));
            ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine("A) Add (Create)"));
            ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine("L) List (Read)"));
            ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine("E) Edit (Update)"));
            ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine("D) Delete"));
            Console.WriteLine(new string('-', 60));
            Console.WriteLine("B) Back (nazad na izbor entiteta)");
            Console.WriteLine();

            while (true){
                Console.Write("Unos: ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input)) continue;

                switch (input.ToUpperInvariant()){
                    case "A": return (false, CrudAction.Add);
                    case "L": return (false, CrudAction.List);
                    case "E": return (false, CrudAction.Edit);
                    case "D": return (false, CrudAction.Delete);
                    case "B": return (true, default);
                    default:
                        ConsoleUI.Warn("Nevažeći izbor. Dozvoljeno: A/L/E/D ili B za nazad.", options);
                        break;
                }
            }
        }
    }
}
