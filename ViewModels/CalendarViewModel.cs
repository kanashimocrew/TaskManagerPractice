using TaskManager.Services;
using TaskManager.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.Views;
using TaskManager.Resources.Localization;

namespace TaskManager.ViewModels
{
    public partial class CalendarViewModel : BaseViewModel
    {
        private readonly IDatabaseService _databaseService;

        private DateTime _currentDate;
        public DateTime CurrentDate
        {
            get => _currentDate;
            set => SetProperty(ref _currentDate, value);
        }

        public ObservableCollection<CalendarDay> Days { get; } = new ObservableCollection<CalendarDay>();

        public CalendarViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            CurrentDate = DateTime.Now;
            InitializeCalendar();
        }

        private async void InitializeCalendar()
        {
            await LoadCalendarDays();
        }

        [RelayCommand]
        private async Task PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            await LoadCalendarDays();
        }

        [RelayCommand]
        private async Task NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            await LoadCalendarDays();
        }

        [RelayCommand]
        private async Task GoToToday()
        {
            CurrentDate = DateTime.Now;
            await LoadCalendarDays();
        }

        [RelayCommand]
        private async Task SelectDay(CalendarDay day)
        {
            if (day != null)
            {
                await Shell.Current.GoToAsync($"{nameof(TaskListPage)}?SelectedDate={day.Date:yyyy-MM-dd}");
            }
        }

        private async Task LoadCalendarDays()
        {
            Days.Clear();

            var year = CurrentDate.Year;
            var month = CurrentDate.Month;

            var taskCounts = await _databaseService.GetTaskCountsForMonthAsync(year, month);

            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var daysInPreviousMonth = (int)firstDayOfMonth.DayOfWeek;
            var previousMonth = firstDayOfMonth.AddMonths(-1);

            for (int i = daysInPreviousMonth - 1; i >= 0; i--)
            {
                var date = firstDayOfMonth.AddDays(-i - 1);
                Days.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = false,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }

            for (int i = 0; i <= lastDayOfMonth.Day - 1; i++)
            {
                var date = firstDayOfMonth.AddDays(i);
                Days.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Now.Date,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }

            var totalCells = 42;
            var remainingCells = totalCells - Days.Count;

            for (int i = 1; i <= remainingCells; i++)
            {
                var date = lastDayOfMonth.AddDays(i);
                Days.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = false,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }
        }
    }
}