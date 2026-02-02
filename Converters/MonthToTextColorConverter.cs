using System.Globalization;

namespace TaskManager.Converters
{
    public class MonthToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrentMonth && isCurrentMonth)
                return Colors.Black;

            return Color.FromArgb("#888888");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}