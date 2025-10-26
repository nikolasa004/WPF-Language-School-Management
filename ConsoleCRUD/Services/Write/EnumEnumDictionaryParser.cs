namespace ConsoleCRUD.Services.Write
{
    internal static class EnumEnumDictionaryParser
    {
        public static bool TryParse(Type keyEnum, Type valEnum, string input, out object? dict)
        {
            dict = null;
            input = (input ?? "").Trim();
            if (string.IsNullOrEmpty(input))
            {
                dict = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyEnum, valEnum));
                return true;
            }
            var normalized = input.Replace("{", "").Replace("}", "").Trim();
            var pairTokens = FormatHelpers.SplitTopLevel(normalized);
            if (pairTokens.Count == 0 && !string.IsNullOrWhiteSpace(normalized))
                pairTokens.Add(normalized);
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyEnum, valEnum);
            var addMi = dictType.GetMethod("Add", new[] { keyEnum, valEnum })!;
            var result = Activator.CreateInstance(dictType)!;
            foreach (var token in pairTokens)
            {
                var t = token.Trim();
                if (t.StartsWith("(") && t.EndsWith(")") && t.Length >= 3)
                    t = t.Substring(1, t.Length - 2).Trim();
                var (left, right, ok) = SplitKeyValue(t);
                if (!ok) return false;
                if (!TryParseEnum(keyEnum, left, out var k)) return false;
                if (!TryParseEnum(valEnum, right, out var v)) return false;
                addMi.Invoke(result, new[] { k, v });
            }
            dict = result;
            return true;
        }
        private static (string left, string right, bool ok) SplitKeyValue(string s)
        {
            string[] delims = { "->", ":", "=", "," };
            foreach (var d in delims)
            {
                var pos = s.IndexOf(d, StringComparison.Ordinal);
                if (pos > 0)
                    return (s[..pos].Trim(), s[(pos + d.Length)..].Trim(), true);
            }
            return ("", "", false);
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
