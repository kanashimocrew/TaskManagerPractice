using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Resources.Localization;

namespace TaskManager.ViewModels
{
    public class TaskEditViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private TaskItem _originalTask;

        private string _title;
        private string _description;
        private DateTime _dueDate;
        private TimeSpan _dueTime;
        private TaskPriority _selectedPriority;
        private bool _hasChanges;
        private bool _isNewTask = true;

        private List<string> _priorityOptions;
        private string _pageTitle;
        private string _saveButtonText;
        private int _selectedPriorityIndex = 1;

        public string Title
        {
            get => _title;
            set
            {
                if (SetProperty(ref _title, value))
                {
                    CheckForChanges();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    CheckForChanges();
                }
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                if (SetProperty(ref _dueDate, value))
                {
                    CheckForChanges();
                }
            }
        }

        public TimeSpan DueTime
        {
            get => _dueTime;
            set
            {
                if (SetProperty(ref _dueTime, value))
                {
                    CheckForChanges();
                }
            }
        }

        public TaskPriority SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                if (SetProperty(ref _selectedPriority, value))
                {
                    CheckForChanges();
                }
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        }

        public bool IsNewTask
        {
            get => _isNewTask;
            set
            {
                if (SetProperty(ref _isNewTask, value))
                {
                    UpdatePageTitle();
                    UpdateSaveButtonText();
                }
            }
        }

        public List<string> PriorityOptions
        {
            get => _priorityOptions;
            set => SetProperty(ref _priorityOptions, value);
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public string SaveButtonText
        {
            get => _saveButtonText;
            set => SetProperty(ref _saveButtonText, value);
        }

        public int SelectedPriorityIndex
        {
            get => _selectedPriorityIndex;
            set
            {
                if (SetProperty(ref _selectedPriorityIndex, value))
                {
                    SelectedPriority = GetPriorityFromIndex(value);
                }
            }
        }

        public ICommand SaveTaskCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand LoadTaskCommand { get; }

        public TaskEditViewModel() : this(new DatabaseService())
        {
        }

        public TaskEditViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            _dueDate = DateTime.Now.Date;
            _dueTime = new TimeSpan(9, 0, 0);
            _selectedPriority = TaskPriority.Medium;
            _isNewTask = true;

            InitializePriorityOptions();

            UpdatePageTitle();
            UpdateSaveButtonText();

            SaveTaskCommand = new Command(async () => await SaveTask());
            GoBackCommand = new Command(async () => await GoBack());
            LoadTaskCommand = new Command<int>(async (taskId) => await LoadTask(taskId));
        }

        public void Initialize(int? taskId, DateTime? selectedDate)
        {
            if (taskId.HasValue && taskId.Value > 0)
            {
                LoadTaskCommand.Execute(taskId.Value);
            }
            else
            {
                IsNewTask = true;
                if (selectedDate.HasValue)
                {
                    DueDate = selectedDate.Value;
                }
            }
        }

        private void InitializePriorityOptions()
        {
            PriorityOptions = new List<string>
            {
                AppResources.PriorityLow,
                AppResources.PriorityMedium,
                AppResources.PriorityHigh
            };
        }

        public void UpdatePageTitle()
        {
            PageTitle = IsNewTask ? AppResources.NewTaskPageTitle : AppResources.EditTaskPageTitle;
        }

        public void UpdateSaveButtonText()
        {
            SaveButtonText = IsNewTask ? AppResources.AddButton : AppResources.SaveButton;
        }

        private TaskPriority GetPriorityFromIndex(int index)
        {
            return index switch
            {
                0 => TaskPriority.Low,
                1 => TaskPriority.Medium,
                2 => TaskPriority.High,
                _ => TaskPriority.Medium
            };
        }

        private int GetIndexFromPriority(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => 0,
                TaskPriority.Medium => 1,
                TaskPriority.High => 2,
                _ => 1
            };
        }

        private async Task LoadTask(int taskId)
        {
            try
            {
                _originalTask = await _databaseService.GetTaskAsync(taskId);

                if (_originalTask != null)
                {
                    Title = _originalTask.Title;
                    Description = _originalTask.Description ?? string.Empty;
                    DueDate = _originalTask.DueDate.Date;
                    DueTime = _originalTask.DueDate.TimeOfDay;
                    SelectedPriority = _originalTask.Priority;

                    SelectedPriorityIndex = GetIndexFromPriority(SelectedPriority);

                    IsNewTask = false;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось загрузить задачу: {ex.Message}", AppResources.Ok);
            }
        }

        private void CheckForChanges()
        {
            if (_originalTask == null)
            {
                HasChanges = !string.IsNullOrWhiteSpace(Title) ||
                            !string.IsNullOrWhiteSpace(Description);
                return;
            }

            HasChanges = Title != _originalTask.Title ||
                        Description != (_originalTask.Description ?? string.Empty) ||
                        DueDate != _originalTask.DueDate.Date ||
                        DueTime != _originalTask.DueDate.TimeOfDay ||
                        SelectedPriority != _originalTask.Priority;
        }

        private bool _isSaving = false;

        private async Task SaveTask()
        {
            if (_isSaving) return;

            try
            {
                _isSaving = true;

                if (string.IsNullOrWhiteSpace(Title))
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                        AppResources.Validation_TitleRequired, AppResources.Ok);
                    return;
                }

                var dueDateTime = DueDate.Add(DueTime);
                if (dueDateTime < DateTime.Now.AddMinutes(-5))
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert(AppResources.Warning,
                        AppResources.Validation_DateInPast,
                        AppResources.SaveButton, AppResources.CancelButton);

                    if (!confirm) return;
                }

                TaskItem task;

                if (!IsNewTask && _originalTask != null)
                {
                    task = _originalTask;
                    task.Title = Title;
                    task.Description = Description;
                    task.DueDate = dueDateTime;
                    task.Priority = SelectedPriority;
                    task.UpdatedAt = DateTime.Now;
                }
                else
                {
                    task = new TaskItem
                    {
                        Title = Title,
                        Description = Description,
                        DueDate = dueDateTime,
                        Priority = SelectedPriority,
                        Status = Models.TaskStatus.New
                    };
                }

                var result = await _databaseService.SaveTaskAsync(task);

                if (result > 0)
                {
                    MessagingCenter.Send(this, "TaskSaved", task.DueDate.Date);

                    await NavigationService.GoBack();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                        "Не удалось сохранить задачу", AppResources.Ok);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось сохранить задачу: {ex.Message}", AppResources.Ok);
            }
            finally
            {
                _isSaving = false;
            }
        }

        private async Task GoBack()
        {
            if (HasChanges)
            {
                bool save = await Application.Current.MainPage.DisplayAlert(AppResources.UnsavedChangesTitle,
                    AppResources.UnsavedChangesMessage,
                    AppResources.SaveButton, AppResources.CancelButton);

                if (save)
                {
                    await SaveTask();
                    return;
                }
            }

            await NavigationService.GoBack();
        }
    }
}