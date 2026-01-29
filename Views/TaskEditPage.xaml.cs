using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskEditPage : ContentPage
    {
        public TaskEditPage(DateTime? selectedDate = null, int? taskId = null)
        {
            InitializeComponent();

            if (BindingContext is TaskEditViewModel viewModel)
            {
                viewModel.Initialize(taskId, selectedDate);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is TaskEditViewModel viewModel && viewModel.IsNewTask)
            {
                viewModel.UpdatePageTitle();
                viewModel.UpdateSaveButtonText();
            }
        }

        private IDictionary<string, object> GetNavigationParameters()
        {
            if (Shell.Current?.CurrentState?.Location is not null)
            {
                var query = Shell.Current.CurrentState.Location.OriginalString;


                if (query.Contains("TaskId=") || query.Contains("SelectedDate="))
                {
                    var parameters = new Dictionary<string, object>();

                    var queryParts = query.Split('?');
                    if (queryParts.Length > 1)
                    {
                        var queryString = queryParts[1];
                        var pairs = queryString.Split('&');

                        foreach (var pair in pairs)
                        {
                            var keyValue = pair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                var key = keyValue[0];
                                var value = keyValue[1];

                                if (key == "TaskId" && int.TryParse(value, out int taskId))
                                {
                                    parameters[key] = taskId;
                                }
                                else if (key == "SelectedDate")
                                {
                                    var decodedValue = Uri.UnescapeDataString(value);
                                    if (DateTime.TryParse(decodedValue, out DateTime date))
                                    {
                                        parameters[key] = date;
                                    }
                                }
                            }
                        }
                    }

                    return parameters.Count > 0 ? parameters : null;
                }
            }

            return null;
        }
    }
}