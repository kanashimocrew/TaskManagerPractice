using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Services;
using TaskManager.Resources.Localization;

namespace TaskManager.ViewModels
{
    public class TaskDetailViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private TaskItem _originalTask;

        private TaskItem _task;
        private bool _isEditing;
        private string _editTitle;
        private string _editDescription;
        private DateTime _editDueDate;
        private TimeSpan _editDueTime;
        private TaskPriority _editPriority;
        private Models.TaskStatus _editStatus;

        private List<string> _priorityOptions;
        private List<string> _statusOptions;

        public TaskItem Task
        {
            get => _task;
            set => SetProperty(ref _task, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string EditTitle
        {
            get => _editTitle;
            set => SetProperty(ref _editTitle, value);
        }

        public string EditDescription
        {
            get => _editDescription;
            set => SetProperty(ref _editDescription, value);
        }

        public DateTime EditDueDate
        {
            get => _editDueDate;
            set => SetProperty(ref _editDueDate, value);
        }

        public TimeSpan EditDueTime
        {
            get => _editDueTime;
            set => SetProperty(ref _editDueTime, value);
        }

        public TaskPriority EditPriority
        {
            get => _editPriority;
            set => SetProperty(ref _editPriority, value);
        }

        public Models.TaskStatus EditStatus
        {
            get => _editStatus;
            set => SetProperty(ref _editStatus, value);
        }

        public List<string> PriorityOptions
        {
            get => _priorityOptions;
            set => SetProperty(ref _priorityOptions, value);
        }

        public List<string> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value);
        }

        public int EditPriorityIndex
        {
            get
            {
                return EditPriority switch
                {
                    TaskPriority.Low => 0,
                    TaskPriority.Medium => 1,
                    TaskPriority.High => 2,
                    _ => 1
                };
            }
            set
            {
                var newPriority = value switch
                {
                    0 => TaskPriority.Low,
                    1 => TaskPriority.Medium,
                    2 => TaskPriority.High,
                    _ => TaskPriority.Medium
                };

                if (EditPriority != newPriority)
                {
                    EditPriority = newPriority;
                    OnPropertyChanged(nameof(EditPriorityIndex));
                }
            }
        }

        public int EditStatusIndex
        {
            get
            {
                return EditStatus switch
                {
                    Models.TaskStatus.New => 0,
                    Models.TaskStatus.InProgress => 1,
                    Models.TaskStatus.Completed => 2,
                    Models.TaskStatus.Cancelled => 3,
                    _ => 0
                };
            }
            set
            {
                var newStatus = value switch
                {
                    0 => Models.TaskStatus.New,
                    1 => Models.TaskStatus.InProgress,
                    2 => Models.TaskStatus.Completed,
                    3 => Models.TaskStatus.Cancelled,
                    _ => Models.TaskStatus.New
                };

                if (EditStatus != newStatus)
                {
                    EditStatus = newStatus;
                    OnPropertyChanged(nameof(EditStatusIndex));
                }
            }
        }

        public ICommand LoadTaskCommand { get; }
        public ICommand StartEditingCommand { get; }
        public ICommand CancelEditingCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand GoBackCommand { get; }

        public TaskDetailViewModel() : this(new DatabaseService())
        {
        }

        public TaskDetailViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            InitializeOptions();

            LoadTaskCommand = new Command<int>(async (taskId) => await LoadTask(taskId));
            StartEditingCommand = new Command(() => StartEditing());
            CancelEditingCommand = new Command(() => CancelEditing());
            SaveChangesCommand = new Command(async () => await SaveChanges());
            DeleteTaskCommand = new Command(async () => await DeleteTask());
            GoBackCommand = new Command(async () => await GoBack());
        }

        public void Initialize(int taskId)
        {
            LoadTaskCommand.Execute(taskId);
        }

        private void InitializeOptions()
        {
            PriorityOptions = new List<string>
            {
                AppResources.PriorityLow,
                AppResources.PriorityMedium,
                AppResources.PriorityHigh
            };

            StatusOptions = new List<string>
            {
                AppResources.StatusNew,
                AppResources.StatusInProgress,
                AppResources.StatusCompleted,
                AppResources.StatusCancelled
            };
        }

        private async Task LoadTask(int taskId)
        {
            try
            {
                Task = await _databaseService.GetTaskAsync(taskId);
                if (Task != null)
                {
                    _originalTask = new TaskItem
                    {
                        Id = Task.Id,
                        Title = Task.Title,
                        Description = Task.Description,
                        DueDate = Task.DueDate,
                        Priority = Task.Priority,
                        Status = Task.Status
                    };

                    EditTitle = Task.Title;
                    EditDescription = Task.Description ?? string.Empty;
                    EditDueDate = Task.DueDate.Date;
                    EditDueTime = Task.DueDate.TimeOfDay;
                    EditPriority = Task.Priority;
                    EditStatus = Task.Status;

                    OnPropertyChanged(nameof(EditPriorityIndex));
                    OnPropertyChanged(nameof(EditStatusIndex));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось загрузить задачу: {ex.Message}", AppResources.Ok);
            }
        }

        private void StartEditing()
        {
            IsEditing = true;
        }

        private void CancelEditing()
        {
            IsEditing = false;

            if (_originalTask != null)
            {
                EditTitle = _originalTask.Title;
                EditDescription = _originalTask.Description ?? string.Empty;
                EditDueDate = _originalTask.DueDate.Date;
                EditDueTime = _originalTask.DueDate.TimeOfDay;
                EditPriority = _originalTask.Priority;
                EditStatus = _originalTask.Status;

                OnPropertyChanged(nameof(EditPriorityIndex));
                OnPropertyChanged(nameof(EditStatusIndex));
            }
        }

        private async Task SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(EditTitle))
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    AppResources.Validation_TitleRequired, AppResources.Ok);
                return;
            }

            var dueDateTime = EditDueDate.Add(EditDueTime);
            if (dueDateTime < DateTime.Now.AddMinutes(-5))
            {
                bool confirm = await Application.Current.MainPage.DisplayAlert(AppResources.Warning,
                    AppResources.Validation_DateInPast,
                    AppResources.SaveButton, AppResources.CancelButton);

                if (!confirm) return;
            }

            try
            {
                Task.Title = EditTitle;
                Task.Description = EditDescription;
                Task.DueDate = dueDateTime;
                Task.Priority = EditPriority;
                Task.Status = EditStatus;
                Task.UpdatedAt = DateTime.Now;

                await _databaseService.SaveTaskAsync(Task);

                _originalTask.Title = EditTitle;
                _originalTask.Description = EditDescription;
                _originalTask.DueDate = dueDateTime;
                _originalTask.Priority = EditPriority;
                _originalTask.Status = EditStatus;

                IsEditing = false;

                OnPropertyChanged(nameof(Task));

                MessagingCenter.Send(this, "TaskUpdated", Task.DueDate.Date);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                    $"Не удалось сохранить изменения: {ex.Message}", AppResources.Ok);
            }
        }

        private async Task DeleteTask()
        {
            if (Task == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                AppResources.ConfirmDeleteTitle,
                string.Format(AppResources.ConfirmDeleteMessage, Task.Title),
                AppResources.DeleteButton,
                AppResources.CancelButton);

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteTaskAsync(Task);

                    MessagingCenter.Send(this, "TaskDeleted", Task.DueDate.Date);

                    await NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert(AppResources.Error,
                        $"Не удалось удалить задачу: {ex.Message}", AppResources.Ok);
                }
            }
        }

        private async Task GoBack()
        {
            if (IsEditing && HasChanges())
            {
                bool save = await Application.Current.MainPage.DisplayAlert(AppResources.UnsavedChangesTitle,
                    AppResources.UnsavedChangesMessage,
                    AppResources.SaveButton, AppResources.CancelButton);

                if (save)
                {
                    await SaveChanges();
                    return;
                }
            }

            await NavigationService.GoBack();
        }

        private bool HasChanges()
        {
            if (Task == null || _originalTask == null) return false;

            return EditTitle != _originalTask.Title ||
                   EditDescription != (_originalTask.Description ?? string.Empty) ||
                   EditDueDate.Date != _originalTask.DueDate.Date ||
                   EditDueTime != _originalTask.DueDate.TimeOfDay ||
                   EditPriority != _originalTask.Priority ||
                   EditStatus != _originalTask.Status;
        }
    }
}