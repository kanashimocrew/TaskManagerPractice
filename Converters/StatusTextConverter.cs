using System.Globalization;
using TaskManager.Models;

using System.Globalization;
using TaskManager.Models;

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
                    Models.TaskStatus.New => "Новая",
                    Models.TaskStatus.InProgress => "В работе",
                    Models.TaskStatus.Completed => "Выполнена",
                    Models.TaskStatus.Cancelled => "Отменена",
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