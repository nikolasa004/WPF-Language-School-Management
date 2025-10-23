using ConsoleCRUD.Services.Read;
using ConsoleCRUD.View.Styling;
using System.Reflection;

namespace ConsoleCRUD.View.Read
{
    // Renders detailed view of a single object, showing all its properties and fields recursively.
    internal class DetailedView
    {
        public static void RenderDetail(Type modelType, object row, int maxDepth, RenderOptions options)
        {
            Console.Clear();
            Title($"{modelType.Name} — detalji", options);
            BuildDetail(row, modelType, "", maxDepth, new HashSet<object>(RefEq.Instance), options);
            Console.WriteLine();
            Console.Write("Enter za povratak...");
            Console.ReadLine();
        }

        private static void BuildDetail(object obj, Type t, string indent, int depth, HashSet<object> seen, RenderOptions options)
        {
            if (obj == null) { ThemedConsole.With(options.Theme, options.Theme.Warn, () => Console.WriteLine(indent + "(null)")); return; }
            if (depth < 0) { Console.WriteLine(indent + obj); return; }

            if (TypeValueHelpers.IsPrimitiveLike(t))
            {
                ThemedConsole.With(options.Theme, options.Theme.Type, () => Console.Write($"[{t.Name}] "));
                ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine(TypeValueHelpers.FormatPrimitive(obj)));
                return;
            }

            if (TypeValueHelpers.IsEnumerableButNotString(t))
            {
                int i = 0;
                foreach (var it in (System.Collections.IEnumerable)obj)
                {
                    var itType = it?.GetType();
                    ThemedConsole.With(options.Theme, options.Theme.Name, () => Console.Write(indent + $"[{i}] "));
                    ThemedConsole.With(options.Theme, options.Theme.Type, () => Console.WriteLine(itType?.Name ?? "null"));
                    if (it != null) BuildDetail(it, itType!, indent + "  ", depth - 1, seen, options);
                    i++;
                }
                if (i == 0) ThemedConsole.With(options.Theme, options.Theme.Info, () => Console.WriteLine(indent + "(prazna kolekcija)"));
                return;
            }

            if (!t.IsValueType)
            {
                if (seen.Contains(obj)) { ThemedConsole.With(options.Theme, options.Theme.Warn, () => Console.WriteLine(indent + "(cycle)")); return; }
                seen.Add(obj);
            }

            foreach (var m in GetMembers(t))
            {
                var val = m.Getter(obj);
                var pt = TypeValueHelpers.Underlying(m.Type);

                ThemedConsole.With(options.Theme, options.Theme.Name, () => Console.Write(indent + m.Name));
                Console.Write(": ");
                ThemedConsole.With(options.Theme, options.Theme.Type, () => Console.Write($"[{pt.Name}] "));

                if (TypeValueHelpers.IsPrimitiveLike(pt))
                {
                    ThemedConsole.With(options.Theme, options.Theme.Value, () => Console.WriteLine(TypeValueHelpers.FormatPrimitive(val)));
                }
                else
                {
                    Console.WriteLine();
                    BuildDetail(val!, pt, indent + "  ", depth - 1, seen, options);
                }
            }

        }

        private static IEnumerable<Member> GetMembers(Type t)
        {
            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead) continue;
                yield return new Member
                {
                    Name = property.Name,
                    Type = TypeValueHelpers.Underlying(property.PropertyType),
                    Getter = (obj) => { try { return obj == null ? null : property.GetValue(obj); } catch { return null; } }
                };
            }
            foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                yield return new Member
                {
                    Name = field.Name,
                    Type = TypeValueHelpers.Underlying(field.FieldType),
                    Getter = (obj) => { try { return obj == null ? null : field.GetValue(obj); } catch { return null; } }
                };
            }
        }

        private static void Title(string title, RenderOptions options)
        {
            var line = new string('-', Math.Max(10, Math.Min(Console.WindowWidth - 4, 72)));
            Console.WriteLine("+" + line + "+");
            int w = Math.Max(10, Math.Min(Console.WindowWidth - 2, 72));
            int pad = Math.Max(0, (w - title.Length) / 2);
            ThemedConsole.With(options.Theme, options.Theme.Header, () => Console.WriteLine(" " + new string(' ', pad) + title));
            Console.WriteLine("+" + line + "+");
        }

        // Helper class to represent a member (property or field) with a getter.
        private sealed class RefEq : IEqualityComparer<object>
        {
            public static readonly RefEq Instance = new();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
