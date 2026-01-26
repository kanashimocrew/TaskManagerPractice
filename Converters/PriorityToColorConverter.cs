using System.Globalization;
using TaskManager.Models;

namespace TaskManager.Converters
{
    public class PriorityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.High => Color.FromArgb("#FFCDD2"), // Красный
                    TaskPriority.Medium => Color.FromArgb("#FFF9C4"), // Желтый
                    TaskPriority.Low => Color.FromArgb("#C8E6C9"), // Зеленый
                    _ => Colors.Transparent
                };
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}