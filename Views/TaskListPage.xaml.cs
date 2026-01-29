using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskListPage : ContentPage
    {
        public TaskListPage()
        {
            InitializeComponent();
        }

        public TaskListPage(DateTime selectedDate) : this()
        {

            if (BindingContext is TaskListViewModel viewModel)
            {
                viewModel.SelectedDate = selectedDate;
            }

            Title = $"Задачи на {selectedDate:dd.MM.yyyy}";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is TaskListViewModel viewModel)
            {
                viewModel.LoadTasksCommand.Execute(null);
            }
        }
    }
}