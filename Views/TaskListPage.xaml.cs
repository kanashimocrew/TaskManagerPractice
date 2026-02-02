using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskListPage : ContentPage
    {
        private readonly DateTime _selectedDate;

        public TaskListPage(DateTime selectedDate)
        {
            InitializeComponent();
            _selectedDate = selectedDate;

            Title = $"Задачи на {selectedDate:dd.MM.yyyy}";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is TaskListViewModel viewModel)
            {

                if (viewModel.SelectedDate.Date != _selectedDate.Date)
                {
                    viewModel.SelectedDate = _selectedDate;
                }

                viewModel.LoadTasksCommand.Execute(null);
            }
        }
    }
}