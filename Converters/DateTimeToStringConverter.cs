using System.Globalization;
using TaskManager.Models;

namespace TaskManager.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime.Date == DateTime.Today)
                {
                    return $"Сегодня, {dateTime:HH:mm}";
                }
                else if (dateTime.Date == DateTime.Today.AddDays(1))
                {
                    return $"Завтра, {dateTime:HH:mm}";
                }
                else if (dateTime.Date == DateTime.Today.AddDays(-1))
                {
                    return $"Вчера, {dateTime:HH:mm}";
                }
                else
                {
                    return dateTime.ToString("dd.MM.yyyy, HH:mm");
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}