using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class TaskDetailPage : ContentPage
    {
        public TaskDetailPage()
        {
            InitializeComponent();
        }

        public TaskDetailPage(int taskId) : this()
        {
            if (BindingContext is TaskDetailViewModel viewModel)
            {
                viewModel.Initialize(taskId);
            }
        }
    }
}