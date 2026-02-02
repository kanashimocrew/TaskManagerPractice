using Microsoft.Maui.Controls;
using TaskManager.ViewModels;
using TaskManager.Views;

namespace TaskManager.Services
{
    public static class NavigationService
    {
        private static INavigation GetNavigation()
        {
            if (Application.Current?.MainPage is NavigationPage navPage)
                return navPage.Navigation;

            return Application.Current?.MainPage?.Navigation;
        }

        public static async Task GoBack()
        {
            var navigation = GetNavigation();
            if (navigation != null)
            {
                await navigation.PopAsync();
            }
        }

        public static async Task NavigateToCreateTask(DateTime selectedDate)
        {
            try
            {
                var navigation = GetNavigation();
                if (navigation == null) return;

                var databaseService = new DatabaseService();
                var viewModel = new TaskEditViewModel(databaseService);
                viewModel.Initialize(null, selectedDate);

                var editPage = new TaskEditPage
                {
                    BindingContext = viewModel
                };

                await navigation.PushAsync(editPage);
            }
            catch (Exception ex)
            {
                await ShowAlert($"Ошибка навигации: {ex.Message}");
            }
        }

        public static async Task NavigateToTaskDetail(int taskId)
        {
            try
            {
                var navigation = GetNavigation();
                if (navigation == null) return;

                var detailPage = new TaskDetailPage(taskId);
                await navigation.PushAsync(detailPage);
            }
            catch (Exception ex)
            {
                await ShowAlert($"Ошибка навигации: {ex.Message}");
            }
        }

        private static async Task ShowAlert(string message)
        {
            try
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", message, "OK");
                }
            }
            catch
            {

            }
        }
    }
}