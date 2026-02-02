using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using TaskManager.Models;
using TaskManager.Services;

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
            get
            {
                return SelectedPriority switch
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

                if (SelectedPriority != newPriority)
                {
                    SelectedPriority = newPriority;
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
                "Низкий",
                "Средний",
                "Высокий"
            };
        }

        public void UpdatePageTitle()
        {
            PageTitle = IsNewTask ? "Новая задача" : "Редактирование задачи";
        }

        public void UpdateSaveButtonText()
        {
            SaveButtonText = IsNewTask ? "Создать" : "Сохранить";
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
                    IsNewTask = false;

                    OnPropertyChanged(nameof(SelectedPriorityIndex));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить задачу: {ex.Message}", "OK");
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
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        "Пожалуйста, введите название задачи", "OK");
                    return;
                }

                var dueDateTime = DueDate.Add(DueTime);
                if (dueDateTime < DateTime.Now.AddMinutes(-5))
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert("Подтверждение",
                        "Выбранная дата уже прошла. Вы уверены, что хотите сохранить задачу с прошедшей датой?",
                        "Сохранить", "Отмена");

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
                    Console.WriteLine($"Задача сохранена: ID={task.Id}, Title={task.Title}");

                    MessagingCenter.Send(this, "TaskSaved", task.DueDate.Date);

                    await NavigationService.GoBack();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        "Не удалось сохранить задачу", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось сохранить задачу: {ex.Message}", "OK");
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
                bool save = await Application.Current.MainPage.DisplayAlert("Сохранение",
                    "У вас есть несохраненные изменения. Хотите сохранить перед выходом?",
                    "Сохранить", "Не сохранять");

                if (save)
                {
                    await SaveTask();
                    return;
                }
            }
NavigationService.GoBack();
        }
    }
}