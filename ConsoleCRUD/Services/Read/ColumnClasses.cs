using System.Collections;
using System.Globalization;

namespace ConsoleCRUD.Services.Read
{
    //Represents a column in the tabular view of entities.
    internal sealed class Column
    {
        public string Header = "";
        public Func<object, string> Getter = _ => "";
        public bool AlignRight;
    }

    //Represents a member (property/field) of an entity type.
    internal sealed class Member
    {
        public string Name = "";
        public Type Type = typeof(object);
        public Func<object, object?> Getter = _ => null;
    }

    //Holds helper methods for type and value handling.
    internal static class TypeValueHelpers
    {
        internal static Type Underlying(Type t) => Nullable.GetUnderlyingType(t) ?? t;

        internal static bool IsPrimitiveLike(Type t)
        {
            t = Underlying(t);
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
                   || t == typeof(DateTime) || t == typeof(DateTimeOffset)
                   || t == typeof(Guid) || t == typeof(TimeSpan);
        }

        internal static bool IsNumericLike(Type t)
        {
            t = Underlying(t);
            return t == typeof(byte) || t == typeof(sbyte) ||
                   t == typeof(short) || t == typeof(ushort) ||
                   t == typeof(int) || t == typeof(uint) ||
                   t == typeof(long) || t == typeof(ulong) ||
                   t == typeof(float) || t == typeof(double) ||
                   t == typeof(decimal);
        }

        internal static bool IsEnumerableButNotString(Type t)
        {
            if (t == typeof(string)) return false;
            return typeof(IEnumerable).IsAssignableFrom(t);
        }

        internal static string FormatPrimitive(object? value)
        {
            if (value == null) return "";
            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                bool b => b ? "true" : "false",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? ""
            };
        }

        internal static string SummarizeEnumerable(IEnumerable? en)
        {
            if (en == null) return "";
            var items = new List<string>();
            int count = 0;
            foreach (var it in en)
            {
                count++;
                if (items.Count < 3) items.Add(Preview(it));
            }
            var head = string.Join(", ", items);
            if (count > 3) head += ", …";
            return $"{count} items: [{head}]";
        }

        private static string Preview(object? obj)
        {
            if (obj == null) return "null";
            var t = obj.GetType();
            if (IsPrimitiveLike(t)) return FormatPrimitive(obj);
            return (t.GetProperty("Email")?.GetValue(obj)
                 ?? t.GetProperty("Ime")?.GetValue(obj)
                 ?? t.GetProperty("Name")?.GetValue(obj)
                 ?? t.GetProperty("Naziv")?.GetValue(obj)
                 ?? t.GetProperty("Id")?.GetValue(obj))?.ToString() ?? t.Name;
        }
    }
}
