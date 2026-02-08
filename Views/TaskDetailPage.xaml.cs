using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

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
    }
}