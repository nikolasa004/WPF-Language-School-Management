using System;
using System.Globalization;
using System.Windows.Data;

namespace LangLang.Converters
{
    public class DateToStringConverter : IValueConverter
    {
        private const string Format = "yyyy-MM-dd";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && DateTime.TryParseExact(s, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            return DateTime.Now;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
                return date.ToString(Format);
            return string.Empty;
        }
    }
}