using TaskManager.ViewModels;
using Microsoft.Maui.Controls;

namespace TaskManager.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        public TaskDetailPage(int taskId)
        {
            InitializeComponent();

            if (BindingContext is TaskDetailViewModel viewModel)
            {
                viewModel.Initialize(taskId);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is TaskDetailViewModel viewModel)
            {

                var parameters = GetNavigationParameters();

                if (parameters != null && parameters.ContainsKey("TaskId"))
                {
                    int taskId = (int)parameters["TaskId"];
                    viewModel.Initialize(taskId);
                }
            }
        }

        private IDictionary<string, object> GetNavigationParameters()
        {
            if (Shell.Current?.CurrentState?.Location is not null)
            {
                var query = Shell.Current.CurrentState.Location.OriginalString;

                if (query.Contains("TaskId="))
                {
                    var parts = query.Split('&');
                    var parameters = new Dictionary<string, object>();

                    foreach (var part in parts)
                    {
                        if (part.Contains("="))
                        {
                            var keyValue = part.Split('=');
                            if (keyValue.Length == 2)
                            {
                                var key = keyValue[0];
                                var value = keyValue[1];

                                if (key == "TaskId" && int.TryParse(value, out int taskId))
                                {
                                    parameters[key] = taskId;
                                }
                            }
                        }
                    }

                    return parameters;
                }
            }

            return null;
        }
    }


    public class PriorityTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Models.TaskPriority priority)
            {
                return priority switch
                {
                    Models.TaskPriority.Low => "Низкий приоритет",
                    Models.TaskPriority.Medium => "Средний приоритет",
                    Models.TaskPriority.High => "Высокий приоритет",
                    _ => "Без приоритета"
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}