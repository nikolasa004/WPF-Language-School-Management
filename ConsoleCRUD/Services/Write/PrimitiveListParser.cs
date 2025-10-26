using System.Collections;

namespace ConsoleCRUD.Services.Write
{
    internal static class PrimitiveListParser
    {
        public static bool TryParse(Type elemType, string input, out object? list)
        {
            list = null;
            input = (input ?? "").Trim();
            if (string.IsNullOrEmpty(input))
            {
                list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType));
                return true;
            }

            if (input.StartsWith("[")) input = input.Trim();
            if (input.StartsWith("[")) input = input[1..];
            if (input.EndsWith("]")) input = input[..^1];

            var tokens = input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => s.Trim())
                              .ToList();

            var result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elemType))!;
            foreach (var tok in tokens)
            {
                if (!PrimitiveParser.TryParse(elemType, tok, out var val)) return false;
                result.Add(val!);
            }
            list = result;
            return true;
        }
    }
}
