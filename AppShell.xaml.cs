using Microsoft.Maui.Controls;

namespace TaskManager
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            RegisterRoutes();
        }

        private void RegisterRoutes()
        {

            Routing.RegisterRoute(nameof(Views.TaskListPage), typeof(Views.TaskListPage));
            Routing.RegisterRoute(nameof(Views.TaskEditPage), typeof(Views.TaskEditPage));
            Routing.RegisterRoute(nameof(Views.TaskDetailPage), typeof(Views.TaskDetailPage));

            Routing.RegisterRoute("TaskList", typeof(Views.TaskListPage));
            Routing.RegisterRoute("TaskEdit", typeof(Views.TaskEditPage));
            Routing.RegisterRoute("TaskDetail", typeof(Views.TaskDetailPage));
        }
    }
}