using ConsoleCRUD.Services.Read;
using System.Reflection;

namespace ConsoleCRUD.View.Read
{
    // Renders detailed view of a single object, showing all its properties and fields recursively.
    internal class DetailedView
    {
        public static void RenderDetail(Type modelType, object row, int maxDepth)
        {
            Console.Clear();
            Title($"{modelType.Name} — detalji");
            BuildDetail(row, modelType, "", maxDepth, new HashSet<object>(RefEq.Instance));
            Console.WriteLine();
            Console.Write("Enter za povratak...");
            Console.ReadLine();
        }

        private static void BuildDetail(object obj, Type t, string indent, int depth, HashSet<object> seen)
        {
            if (obj == null) { Color.WriteWarn(indent + "(null)"); return; }
            if (depth < 0) { Console.WriteLine(indent + obj); return; }

            if (TypeValueHelpers.IsPrimitiveLike(t))
            {
                Color.With(ConsoleColor.Green, () => Console.Write(indent));
                Color.With(ConsoleColor.Cyan, () => Console.Write($"[{t.Name}] "));
                Color.With(ConsoleColor.White, () => Console.WriteLine(TypeValueHelpers.FormatPrimitive(obj)));
                return;
            }

            if (TypeValueHelpers.IsEnumerableButNotString(t))
            {
                int i = 0;
                foreach (var it in (System.Collections.IEnumerable)obj)
                {
                    var itType = it?.GetType();
                    Color.With(ConsoleColor.Green, () => Console.Write(indent + $"[{i}] "));
                    Color.With(ConsoleColor.Cyan, () => Console.WriteLine(itType?.Name ?? "null"));
                    if (it != null) BuildDetail(it, itType!, indent + "  ", depth - 1, seen);
                    i++;
                }
                if (i == 0) Color.WriteInfo(indent + "(prazna kolekcija)");
                return;
            }

            if (!t.IsValueType)
            {
                if (seen.Contains(obj)) { Color.WriteWarn(indent + "(cycle)"); return; }
                seen.Add(obj);
            }

            foreach (var m in GetMembers(t))
            {
                var val = m.Getter(obj);
                var pt = TypeValueHelpers.Underlying(m.Type);

                Color.With(ConsoleColor.Yellow, () => Console.Write(indent + m.Name));
                Console.Write(": ");


                Color.With(ConsoleColor.Cyan, () => Console.Write($"[{pt.Name}] "));

                if (TypeValueHelpers.IsPrimitiveLike(pt))
                {
                    Color.With(ConsoleColor.White, () => Console.WriteLine(TypeValueHelpers.FormatPrimitive(val)));
                }
                else
                {
                    Console.WriteLine();
                    BuildDetail(val!, pt, indent + "  ", depth - 1, seen);
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

        private static void Title(string title)
        {
            var line = new string('-', Math.Max(10, Math.Min(Console.WindowWidth - 4, 72)));
            Console.WriteLine("+" + line + "+");
            Center(title);
            Console.WriteLine("+" + line + "+");
        }

        private static void Center(string text)
        {
            int w = Math.Max(10, Math.Min(Console.WindowWidth - 2, 72));
            int pad = Math.Max(0, (w - text.Length) / 2);
            Console.WriteLine(" " + new string(' ', pad) + text);
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
