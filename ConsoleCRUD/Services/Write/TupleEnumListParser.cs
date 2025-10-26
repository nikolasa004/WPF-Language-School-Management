using System.Collections;

namespace ConsoleCRUD.Services.Write
{
    internal static class TupleEnumListParser
    {
        public static bool TryParse(Type enumA, Type enumB, string input, out object? list)
        {
            list = null;
            input = (input ?? "").Trim();

            if (string.IsNullOrEmpty(input))
            {
                var tupleType = typeof(Tuple<,>).MakeGenericType(enumA, enumB);
                list = Activator.CreateInstance(typeof(List<>).MakeGenericType(tupleType));
                return true;
            }
            var normalized = input.Replace("[", "").Replace("]", "").Trim();
            var pairTokens = FormatHelpers.SplitTopLevel(normalized);
            if (pairTokens.Count == 0 && !string.IsNullOrWhiteSpace(normalized))
                pairTokens.Add(normalized);
            var tupleT = typeof(Tuple<,>).MakeGenericType(enumA, enumB);
            var result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(tupleT))!;
            foreach (var token in pairTokens)
            {
                var t = token.Trim();
                if (t.StartsWith("(") && t.EndsWith(")") && t.Length >= 3)
                    t = t.Substring(1, t.Length - 2).Trim();
                var delimPos = FindFirstDelimiter(t, new[] { ':', '-', ',' });
                if (delimPos <= 0) return false;
                var left = t[..delimPos].Trim();
                var right = t[(delimPos + 1)..].Trim();
                if (!TryParseEnum(enumA, left, out var aVal)) return false;
                if (!TryParseEnum(enumB, right, out var bVal)) return false;
                result.Add(Activator.CreateInstance(tupleT, aVal, bVal)!);
            }
            list = result;
            return true;
        }
        private static int FindFirstDelimiter(string s, char[] delims)
        {
            foreach (var ch in delims)
            {
                var pos = s.IndexOf(ch);
                if (pos > 0) return pos;
            }
            return -1;
        }
        private static bool TryParseEnum(Type enumType, string raw, out object? value)
        {
            value = null;
            var s = (raw ?? "").Trim();
            if (long.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var asNum))
            {
                value = Enum.ToObject(enumType, asNum);
                return true;
            }
            if (Enum.TryParse(enumType, s, ignoreCase: true, out var parsed))
            {
                value = parsed;
                return true;
            }
            var names = Enum.GetNames(enumType);
            var candidates = names.Where(n => n.StartsWith(s, StringComparison.OrdinalIgnoreCase)).ToList();
            if (candidates.Count == 1)
            {
                value = Enum.Parse(enumType, candidates[0], ignoreCase: true);
                return true;
            }
            return false;
        }
    }
}
