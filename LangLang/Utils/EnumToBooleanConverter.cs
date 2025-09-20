using System;
using System.Windows.Data;

namespace LangLang.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string? enumValue = value?.ToString();
            string? targetValue = parameter?.ToString();
            return enumValue != null && targetValue != null && enumValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return Binding.DoNothing;

            bool useValue = (bool)value;
            if (useValue)
                return Enum.Parse(targetType, parameter.ToString()!);

            return Binding.DoNothing;
        }
    }
}