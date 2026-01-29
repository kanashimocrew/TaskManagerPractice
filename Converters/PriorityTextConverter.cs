using System.Globalization;
using TaskManager.Models;

namespace TaskManager.Converters
{
    public class PriorityTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.None => "Без приоритета",
                    TaskPriority.Low => "Низкий",
                    TaskPriority.Medium => "Средний",
                    TaskPriority.High => "Высокий",
                    _ => "Неизвестно"
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}