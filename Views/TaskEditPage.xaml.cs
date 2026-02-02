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
    }
}