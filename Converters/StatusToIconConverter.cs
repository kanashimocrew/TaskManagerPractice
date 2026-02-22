using System.Globalization;

namespace TaskManager.Converters
{
    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.TaskStatus status)
            {
                return status switch
                {
                    Models.TaskStatus.New => "⏳", 
                    Models.TaskStatus.InProgress => "⚙️", 
                    Models.TaskStatus.Completed => "✅", 
                    Models.TaskStatus.Cancelled => "❌", 
                    _ => "📝" 
                };
            }

            return "📝";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}