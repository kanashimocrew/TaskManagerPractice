using Microsoft.Maui.Controls;
using TaskManager.ViewModels;
using TaskManager.Views;

namespace TaskManager.Services
{
    public static class NavigationService
    {
        public static async Task NavigateToCreateTask(DateTime selectedDate)
        {
            try
            {
                Console.WriteLine($"NavigationService: Начинаем навигацию для даты {selectedDate}");

                
                var currentPage = GetCurrentPage();
                if (currentPage == null)
                {
                    Console.WriteLine("NavigationService: Текущая страница не найдена");
                    return;
                }

                Console.WriteLine($"NavigationService: Текущая страница: {currentPage.GetType().Name}");

                
                var databaseService = new DatabaseService();
                var viewModel = new TaskEditViewModel(databaseService);
                viewModel.Initialize(null, selectedDate);

                var editPage = new TaskEditPage(viewModel);

                
                await currentPage.Navigation.PushAsync(editPage);

                Console.WriteLine("NavigationService: Навигация успешно завершена");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"NavigationService Ошибка: {ex}");
                await ShowAlert($"Ошибка навигации: {ex.Message}");
            }
        }

        public static async Task NavigateToTaskDetail(int taskId)
        {
            try
            {
                var currentPage = GetCurrentPage();
                if (currentPage == null) return;

                var databaseService = new DatabaseService();
                var viewModel = new TaskDetailViewModel(databaseService);
                viewModel.Initialize(taskId);

                var detailPage = new TaskDetailPage(viewModel);
                await currentPage.Navigation.PushAsync(detailPage);
            }
            catch (Exception ex)
            {
                await ShowAlert($"Ошибка навигации: {ex.Message}");
            }
        }

        private static Page GetCurrentPage()
        {
            if (Application.Current?.MainPage == null)
                return null;

            
            if (Application.Current.MainPage is NavigationPage navPage)
            {
                return navPage.CurrentPage;
            }

            
            return Application.Current.MainPage;
        }

        private static async Task ShowAlert(string message)
        {
            try
            {
                var currentPage = GetCurrentPage();
                if (currentPage != null)
                {
                    await currentPage.DisplayAlert("Ошибка", message, "OK");
                }
            }
            catch
            {
                // Игнорируем ошибки показа алерта
            }
        }
    }
}