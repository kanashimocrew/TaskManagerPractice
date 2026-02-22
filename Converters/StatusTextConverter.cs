using System.Globalization;
using TaskManager.Resources.Localization;

namespace TaskManager.Converters
{
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TaskStatus status)
            {
                return status switch
                {
                    Models.TaskStatus.New => AppResources.StatusNew,
                    Models.TaskStatus.InProgress => AppResources.StatusInProgress,
                    Models.TaskStatus.Completed => AppResources.StatusCompleted,
                    Models.TaskStatus.Cancelled => AppResources.StatusCancelled,
                    _ => string.Empty
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