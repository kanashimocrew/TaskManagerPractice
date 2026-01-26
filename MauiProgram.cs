using Microsoft.Extensions.Logging;
using TaskManager.Services;
using TaskManager.ViewModels;
using TaskManager.Views;

namespace TaskManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialDesignIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif


            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();


            builder.Services.AddTransient<CalendarViewModel>();
            builder.Services.AddTransient<TaskListViewModel>();
            builder.Services.AddTransient<TaskEditViewModel>();
            builder.Services.AddTransient<TaskDetailViewModel>();

            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<TaskListPage>();
            builder.Services.AddTransient<TaskEditPage>();
            builder.Services.AddTransient<TaskDetailPage>();

            return builder.Build();
        }
    }
}