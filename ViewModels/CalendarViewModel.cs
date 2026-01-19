using TaskManager.Services;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using TaskManager.Views;

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
                // Навигация на страницу с задачами на выбранный день
                await Shell.Current.GoToAsync($"{nameof(TaskListPage)}?SelectedDate={day.Date:yyyy-MM-dd}");
            }
        }

        private async Task LoadCalendarDays()
        {
            Days.Clear();

            var year = CurrentDate.Year;
            var month = CurrentDate.Month;

            // Получаем количество задач для каждого дня месяца
            var taskCounts = await _databaseService.GetTaskCountsForMonthAsync(year, month);

            // Первый день месяца
            var firstDayOfMonth = new DateTime(year, month, 1);
            // Последний день месяца
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Дни предыдущего месяца для заполнения сетки
            var daysInPreviousMonth = (int)firstDayOfMonth.DayOfWeek;
            var previousMonth = firstDayOfMonth.AddMonths(-1);

            // Добавляем дни предыдущего месяца
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

            // Добавляем дни текущего месяца
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

            // Добавляем дни следующего месяца для завершения сетки
            var totalCells = 42; // 6 недель * 7 дней
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

        public class CalendarDay
        {
            public DateTime Date { get; set; }
            public int DayNumber { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public int TaskCount { get; set; }
        }
    }
}