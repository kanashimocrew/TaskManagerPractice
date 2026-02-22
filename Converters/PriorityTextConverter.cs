using System.Globalization;
using TaskManager.Models;
using TaskManager.Resources.Localization;

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
                    TaskPriority.Low => AppResources.PriorityLow,
                    TaskPriority.Medium => AppResources.PriorityMedium,
                    TaskPriority.High => AppResources.PriorityHigh,
                    _ => AppResources.PriorityNone
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