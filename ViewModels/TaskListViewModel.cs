using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TaskManager.Models;
using TaskManager.Services;

namespace TaskManager.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;
        private DateTime _selectedDate;
        private bool _isLoading;
        private bool _hasTasks;

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    LoadTasks();
                }
            }
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

            LoadTasksCommand = new Command(async () => await LoadTasks());
            DeleteTaskCommand = new Command<TaskItem>(async (task) => await DeleteTask(task));
            ChangeStatusCommand = new Command<TaskItem>(async (task) => await ChangeStatus(task));
            NavigateToCreateTaskCommand = new Command(async () => await NavigateToCreateTask());
            NavigateToTaskDetailCommand = new Command<TaskItem>(async (task) => await NavigateToTaskDetail(task));

            MessagingCenter.Subscribe<object, DateTime>(this, "TaskSaved", (sender, date) =>
            {
                if (date == SelectedDate)
                {
                    LoadTasks();
                }
            });

            MessagingCenter.Subscribe<object, DateTime>(this, "TaskUpdated", (sender, date) =>
            {
                if (date == SelectedDate)
                {
                    LoadTasks();
                }
            });

            MessagingCenter.Subscribe<object, DateTime>(this, "TaskDeleted", (sender, date) =>
            {
                if (date == SelectedDate)
                {
                    LoadTasks();
                }
            });
        }

        private async Task LoadTasks()
        {
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
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось загрузить задачи: {ex.Message}", "OK");
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
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить задачу \"{task.Title}\"?",
                "Удалить",
                "Отмена");

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteTaskAsync(task);
                    Tasks.Remove(task);
                    HasTasks = Tasks.Count > 0;

                    MessagingCenter.Send(this, "TasksUpdated", SelectedDate);
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        $"Не удалось удалить задачу: {ex.Message}", "OK");
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
                MessagingCenter.Send(this, "TaskStatusChanged", task);

                int index = Tasks.IndexOf(task);
                Tasks[index] = task;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Не удалось обновить статус: {ex.Message}", "OK");
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