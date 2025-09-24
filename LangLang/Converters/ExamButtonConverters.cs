using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using LangLang.Controller.Services;
using LangLang.DTO;
using LangLang.Model.Enums;

namespace LangLang.Converters
{
    public class ExamButtonContentConverter : IValueConverter
    {
        private static readonly ExamButtonService _buttonService = new ExamButtonService();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExamTermDTO exam)
            {
                var buttonInfo = _buttonService.GetButtonInfo(exam);
                return buttonInfo.Content;
            }
            return "Not Available";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExamButtonEnabledConverter : IValueConverter
    {
        private static readonly ExamButtonService _buttonService = new ExamButtonService();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExamTermDTO exam)
            {
                var buttonInfo = _buttonService.GetButtonInfo(exam);
                return buttonInfo.IsEnabled;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExamButtonBackgroundConverter : IValueConverter
    {
        private static readonly ExamButtonService _buttonService = new ExamButtonService();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExamTermDTO exam)
            {
                var buttonInfo = _buttonService.GetButtonInfo(exam);

                // If it's not available, return gray brush
                if (!buttonInfo.IsEnabled)
                {
                    return new SolidColorBrush(Colors.Gray);
                }

                // Otherwise return the default image brush
                return null;
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExamButtonStateConverter : IValueConverter
    {
        private static readonly ExamButtonService _buttonService = new ExamButtonService();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExamTermDTO exam)
            {
                return _buttonService.GetButtonState(exam);
            }
            return ExamButtonState.NotAvailable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
