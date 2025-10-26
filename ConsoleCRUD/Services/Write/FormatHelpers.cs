using System.Collections;
using System.Text;
using ConsoleCRUD.Services.Read;

namespace ConsoleCRUD.Services.Write
{
    internal static class FormatHelpers
    {
        public static string FormatTupleList(IEnumerable? en, Type enumA, Type enumB)
        {
            var seq = en ?? Array.Empty<object>();
            var items = new List<string>();
            foreach (var item in seq)
            {
                if (item == null) continue;
                var aProp = item.GetType().GetProperty("Item1");
                var bProp = item.GetType().GetProperty("Item2");
                var a = aProp?.GetValue(item);
                var b = bProp?.GetValue(item);
                items.Add($"{a}:{b}");
            }
            return "[" + string.Join(", ", items) + "]";
        }
        public static string FormatEnumEnumDictionary(IEnumerable? en, Type keyEnum, Type valEnum)
        {
            var seq = en ?? Array.Empty<object>();
            var items = new List<string>();
            foreach (var item in seq)
            {
                if (item == null) continue;
                var t = item.GetType();
                var keyProp = t.GetProperty("Key");
                var valProp = t.GetProperty("Value");
                var k = keyProp?.GetValue(item);
                var v = valProp?.GetValue(item);
                items.Add($"{k}:{v}");
            }
            return "{ " + string.Join(", ", items) + " }";
        }
        public static string FormatList(IEnumerable? en)
        {
            var seq = en ?? Array.Empty<object>();
            var items = new List<string>();
            foreach (var x in seq) items.Add(TypeValueHelpers.FormatPrimitive(x ?? ""));
            return "[" + string.Join(", ", items) + "]";
        }
        public static string PrettyType(Type t)
        {
            if (t == typeof(DateTime)) return "DateTime (yyyy-MM-dd[ HH:mm] | dd.MM.yyyy[ HH:mm])";
            if (t.IsEnum) return $"Enum ({string.Join('/', Enum.GetNames(t))})";
            if (t == typeof(Guid)) return "Guid";
            return t.Name;
        }
        public static List<string> SplitTopLevel(string s)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();
            int depthRound = 0;
            foreach (var ch in s)
            {
                if (ch == '(') { depthRound++; sb.Append(ch); continue; }
                if (ch == ')') { depthRound = Math.Max(0, depthRound - 1); sb.Append(ch); continue; }

                if ((ch == ',' || ch == ';') && depthRound == 0)
                {
                    var tok = sb.ToString().Trim();
                    if (tok.Length > 0) tokens.Add(tok);
                    sb.Clear();
                }
                else sb.Append(ch);
            }
            var last = sb.ToString().Trim();
            if (last.Length > 0) tokens.Add(last);
            return tokens;
        }
        public static string[] GetEnumExamples(Type enumType, int count)
            => Enum.GetNames(enumType).Take(Math.Max(1, count)).ToArray();
    }}
