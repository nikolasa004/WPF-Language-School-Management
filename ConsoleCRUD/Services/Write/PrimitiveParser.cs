using System.Globalization;

namespace ConsoleCRUD.Services.Write
{
    internal static class PrimitiveParser
    {
        public static bool TryParse(Type t, string? input, out object? value)
        {
            value = null;
            input = (input ?? "").Trim();
            var u = Nullable.GetUnderlyingType(t) ?? t;

            if (u == typeof(string)) { value = input; return true; }

            if (u == typeof(int)) { if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(long)) { if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(short)) { if (short.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(byte)) { if (byte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(ulong)) { if (ulong.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(uint)) { if (uint.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(ushort)) { if (ushort.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(sbyte)) { if (sbyte.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }

            if (u == typeof(double)) { if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(float)) { if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }
            if (u == typeof(decimal)) { if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var v)) { value = v; return true; } }

            if (u == typeof(bool))
            {
                var s = input.ToLowerInvariant();
                if (s is "true" or "t" or "1" or "yes" or "y" or "da" or "d") { value = true; return true; }
                if (s is "false" or "f" or "0" or "no" or "n" or "ne") { value = false; return true; }
                return false;
            }

            if (u == typeof(char)) { if (input.Length == 1) { value = input[0]; return true; } }

            if (u == typeof(DateTime))
            {
                string[] formats = { "yyyy-MM-dd HH:mm", "yyyy-MM-dd", "dd.MM.yyyy HH:mm", "dd.MM.yyyy" };
                if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var v1)) { value = v1; return true; }
                if (DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var v2)) { value = v2; return true; }
                return false;
            }

            if (u.IsEnum)
            {
                if (long.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asNum))
                {
                    value = Enum.ToObject(u, asNum);
                    return true;
                }
                if (Enum.TryParse(u, input, ignoreCase: true, out var v)) { value = v; return true; }

                var names = Enum.GetNames(u);
                var candidates = names.Where(n => n.StartsWith(input, StringComparison.OrdinalIgnoreCase)).ToList();
                if (candidates.Count == 1)
                {
                    value = Enum.Parse(u, candidates[0], ignoreCase: true);
                    return true;
                }
                return false;
            }
            if (u == typeof(Guid)) return Guid.TryParse(input, out var g) ? (value = g) != null : false;

            return false;
        }
    }
}
