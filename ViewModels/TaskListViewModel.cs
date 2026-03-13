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
        private string _searchText;
        private List<TaskItem> _allTasks = new List<TaskItem>();
        private bool _isSelectionMode;
        private bool _isAllSelected;
        private int _selectedCount;

        // Поиск
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTasks();
                }
            }
        }

        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                if (SetProperty(ref _isSelectionMode, value))
                {
                    if (!value)
                    {
                        ClearSelection();
                    }
                    OnPropertyChanged(nameof(IsSelectionModeText));
                }
            }
        }

        public string IsSelectionModeText => IsSelectionMode ? "Отменить" : "Выбрать";

        public bool IsAllSelected
        {
            get => _isAllSelected;
            set
            {
                if (SetProperty(ref _isAllSelected, value))
                {
                    SelectAllTasks(value);
                }
            }
        }

        public int SelectedCount
        {
            get => _selectedCount;
            set => SetProperty(ref _selectedCount, value);
        }

        public bool HasSelectedTasks => SelectedCount > 0;

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
        public ObservableCollection<TaskItem> SelectedTasks { get; } = new ObservableCollection<TaskItem>();

        public ICommand LoadTasksCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ChangeStatusCommand { get; }
        public ICommand NavigateToCreateTaskCommand { get; }
        public ICommand NavigateToTaskDetailCommand { get; }
        public ICommand ToggleSelectionModeCommand { get; }
        public ICommand DeleteSelectedTasksCommand { get; }
        public ICommand SelectTaskCommand { get; }

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
            ToggleSelectionModeCommand = new Command(() => ToggleSelectionMode());
            DeleteSelectedTasksCommand = new Command(async () => await DeleteSelectedTasks());
            SelectTaskCommand = new Command<TaskItem>((task) => SelectTask(task));

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
                _allTasks = await _databaseService.GetTasksByDateAsync(SelectedDate);

                FilterTasks();

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

        private void FilterTasks()
        {
            Tasks.Clear();

            var filteredTasks = _allTasks;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredTasks = _allTasks
                    .Where(t => t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (var task in filteredTasks)
            {
                Tasks.Add(task);
            }

            HasTasks = Tasks.Count > 0;

            UpdateSelectionStatus();
        }

        private void ToggleSelectionMode()
        {
            IsSelectionMode = !IsSelectionMode;
        }

        private void SelectTask(TaskItem task)
        {
            if (!IsSelectionMode) return;

            if (SelectedTasks.Contains(task))
            {
                SelectedTasks.Remove(task);
            }
            else
            {
                SelectedTasks.Add(task);
            }

            SelectedCount = SelectedTasks.Count;
            IsAllSelected = SelectedCount == Tasks.Count;
            OnPropertyChanged(nameof(HasSelectedTasks));
        }

        private void SelectAllTasks(bool select)
        {
            if (select)
            {
                foreach (var task in Tasks)
                {
                    if (!SelectedTasks.Contains(task))
                    {
                        SelectedTasks.Add(task);
                    }
                }
            }
            else
            {
                SelectedTasks.Clear();
            }

            SelectedCount = SelectedTasks.Count;
            OnPropertyChanged(nameof(HasSelectedTasks));
        }

        private void ClearSelection()
        {
            SelectedTasks.Clear();
            SelectedCount = 0;
            IsAllSelected = false;
            OnPropertyChanged(nameof(HasSelectedTasks));
        }

        private void UpdateSelectionStatus()
        {
            if (!IsSelectionMode) return;

            var tasksToRemove = SelectedTasks.Where(t => !Tasks.Contains(t)).ToList();
            foreach (var task in tasksToRemove)
            {
                SelectedTasks.Remove(task);
            }

            SelectedCount = SelectedTasks.Count;
            IsAllSelected = SelectedCount == Tasks.Count && Tasks.Count > 0;
            OnPropertyChanged(nameof(HasSelectedTasks));
        }

        private async Task DeleteSelectedTasks()
        {
            if (SelectedTasks.Count == 0) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить {SelectedTasks.Count} задач?",
                "Удалить",
                "Отмена");

            if (confirm)
            {
                try
                {
                    IsLoading = true;

                    foreach (var task in SelectedTasks.ToList())
                    {
                        await _databaseService.DeleteTaskAsync(task);
                        Tasks.Remove(task);
                        _allTasks.Remove(task);
                    }

                    SelectedTasks.Clear();
                    SelectedCount = 0;
                    IsAllSelected = false;
                    HasTasks = Tasks.Count > 0;

                    OnPropertyChanged(nameof(HasSelectedTasks));

                    IsSelectionMode = false;
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                        $"Не удалось удалить задачи: {ex.Message}", AppResources.Ok);
                }
                finally
                {
                    IsLoading = false;
                }
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
                    _allTasks.Remove(task);
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

                int allIndex = _allTasks.FindIndex(t => t.Id == task.Id);
                if (allIndex >= 0)
                {
                    _allTasks[allIndex] = task;
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

            if (IsSelectionMode)
            {
                SelectTask(task);
                return;
            }

            await Services.NavigationService.NavigateToTaskDetail(task.Id);
        }
    }
}