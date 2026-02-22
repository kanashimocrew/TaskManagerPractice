using System.Collections.ObjectModel;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Resources.Localization;

namespace TaskManager.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private DateTime _selectedDate;
        private bool _isLoading;
        private bool _hasTasks;
        private string _pageTitle;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    UpdatePageTitle(); 
                }
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public bool HasTasks
        {
            get => _hasTasks;
            set
            {
                if (_hasTasks != value)
                {
                    _hasTasks = value;
                    OnPropertyChanged(nameof(HasTasks));
                }
            }
        }

        public ObservableCollection<TaskItem> Tasks { get; } = new ObservableCollection<TaskItem>();

        public ICommand LoadTasksCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand NavigateToCreateTaskCommand { get; }
        public ICommand NavigateToTaskDetailCommand { get; }

        public TaskListViewModel() : this(new DatabaseService())
        {
        }

        public TaskListViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            SelectedDate = DateTime.Now.Date;

            AppResources.Culture = System.Globalization.CultureInfo.CurrentUICulture;
            UpdatePageTitle();

            LoadTasksCommand = new Command(async () => await LoadTasks());

            DeleteTaskCommand = new Command<TaskItem>(async (task) => await DeleteTask(task));
            ChangeStatusCommand = new Command<TaskItem>(async (task) => await ChangeStatus(task));
            NavigateToCreateTaskCommand = new Command(async () => await NavigateToCreateTask());
            NavigateToTaskDetailCommand = new Command<TaskItem>(async (task) => await NavigateToTaskDetail(task));

            MessagingCenter.Subscribe<object>(this, "TaskSaved", (sender) =>
            {
                LoadTasksCommand.Execute(null);
            });

            MessagingCenter.Subscribe<object>(this, "TaskUpdated", (sender) =>
            {
                LoadTasksCommand.Execute(null);
            });

            MessagingCenter.Subscribe<object>(this, "TaskDeleted", (sender) =>
            {
                LoadTasksCommand.Execute(null);
            });

            MessagingCenter.Subscribe<object>(this, "LanguageChanged", (sender) =>
            {
                UpdatePageTitle();
            });
        }

        private void UpdatePageTitle()
        {
            PageTitle = string.Format(AppResources.TasksPageTitle, SelectedDate.ToString("dd.MM.yyyy"));
        }

        private async Task LoadTasks()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Tasks.Clear();

                var taskList = await _databaseService.GetTasksByDateAsync(SelectedDate);

                foreach (var task in taskList)
                {
                    Tasks.Add(task);
                }

                HasTasks = Tasks.Count > 0;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось загрузить задачи: {ex.Message}", AppResources.Ok);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTask(TaskItem task)
        {
            if (task == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                AppResources.ConfirmDeleteTitle,
                string.Format(AppResources.ConfirmDeleteMessage, task.Title),
                AppResources.DeleteButton,
                AppResources.CancelButton);

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteTaskAsync(task);
                    Tasks.Remove(task);
                    HasTasks = Tasks.Count > 0;
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                        $"Не удалось удалить задачу: {ex.Message}", AppResources.Ok);
                }
            }
        }

        private async Task ChangeStatus(TaskItem task)
        {
            if (task == null) return;

            if (task.Status == Models.TaskStatus.New || task.Status == Models.TaskStatus.InProgress)
            {
                task.Status = Models.TaskStatus.Completed;
            }
            else
            {
                task.Status = Models.TaskStatus.New;
            }

            task.UpdatedAt = DateTime.Now;

            try
            {
                await _databaseService.SaveTaskAsync(task);

                int index = Tasks.IndexOf(task);
                if (index >= 0)
                {
                    Tasks[index] = task;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось обновить статус: {ex.Message}", AppResources.Ok);
            }
        }

        private async Task NavigateToCreateTask()
        {
            await Services.NavigationService.NavigateToCreateTask(SelectedDate);
        }

        private async Task NavigateToTaskDetail(TaskItem task)
        {
            if (task == null) return;
            await Services.NavigationService.NavigateToTaskDetail(task.Id);
        }
    }
}