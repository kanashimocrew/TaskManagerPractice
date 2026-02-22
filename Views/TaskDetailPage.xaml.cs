using TaskManager.ViewModels;
using TaskManager.Converters;

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