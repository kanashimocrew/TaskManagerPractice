using System;
using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskListPage : ContentPage
    {
        public TaskListPage(TaskListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            Title = $"Задачи на {viewModel.SelectedDate:dd.MM.yyyy}";
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