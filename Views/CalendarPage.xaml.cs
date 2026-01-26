using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class CalendarPage : ContentPage
    {
        private DateTime _currentDate;
        private List<CalendarDay> _calendarDays = new List<CalendarDay>();

        public CalendarPage()
        {
            InitializeComponent();
            _currentDate = DateTime.Now;


            PrevMonthButton.Clicked += OnPrevMonthClicked;
            NextMonthButton.Clicked += OnNextMonthClicked;
            TodayButton.Clicked += OnTodayClicked;

            InitializeCalendar();
        }

        private void InitializeCalendar()
        {
            UpdateMonthYearLabel();
            LoadCalendarData();
            RenderCalendar();
        }

        private void UpdateMonthYearLabel()
        {

            string[] russianMonths = {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };

            int monthIndex = _currentDate.Month - 1;
            MonthYearLabel.Text = $"{russianMonths[monthIndex]} {_currentDate.Year}";
        }

        private void LoadCalendarData()
        {
            _calendarDays.Clear();

            int year = _currentDate.Year;
            int month = _currentDate.Month;


            Dictionary<DateTime, int> taskCounts = GenerateTestTaskCounts(year, month);

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);


            int firstDayOffset = ((int)firstDayOfMonth.DayOfWeek - 1 + 7) % 7;


            for (int i = firstDayOffset - 1; i >= 0; i--)
            {
                DateTime date = firstDayOfMonth.AddDays(-i - 1);
                _calendarDays.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = false,
                    IsToday = false,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }


            for (int i = 0; i < lastDayOfMonth.Day; i++)
            {
                DateTime date = firstDayOfMonth.AddDays(i);
                _calendarDays.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = true,
                    IsToday = date.Date == DateTime.Now.Date,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }


            int totalCells = 42; 
            int remainingCells = totalCells - _calendarDays.Count;

            for (int i = 1; i <= remainingCells; i++)
            {
                DateTime date = lastDayOfMonth.AddDays(i);
                _calendarDays.Add(new CalendarDay
                {
                    Date = date,
                    DayNumber = date.Day,
                    IsCurrentMonth = false,
                    IsToday = false,
                    TaskCount = taskCounts.ContainsKey(date) ? taskCounts[date] : 0
                });
            }
        }

        private Dictionary<DateTime, int> GenerateTestTaskCounts(int year, int month)
        {
            Dictionary<DateTime, int> counts = new Dictionary<DateTime, int>();
            Random random = new Random();


            for (int i = 0; i < 15; i++)
            {
                int randomDay = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                DateTime date = new DateTime(year, month, randomDay);
                counts[date] = random.Next(1, 6); 
            }


            if (year == DateTime.Now.Year && month == DateTime.Now.Month)
            {
                counts[DateTime.Now.Date] = random.Next(1, 4);
            }


            counts[new DateTime(year, month, 5)] = 8;
            counts[new DateTime(year, month, 15)] = 12;
            counts[new DateTime(year, month, 25)] = 5;

            return counts;
        }

        private void RenderCalendar()
        {

            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();


            for (int row = 0; row < 6; row++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = 70 });
            }

            for (int col = 0; col < 7; col++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            for (int i = 0; i < _calendarDays.Count; i++)
            {
                int row = i / 7;
                int col = i % 7;

                CalendarDay day = _calendarDays[i];
                Frame dayCell = CreateDayCell(day);

                CalendarGrid.Add(dayCell, col, row);
            }
        }

        private Frame CreateDayCell(CalendarDay day)
        {

            Frame cellFrame = new Frame
            {
                BorderColor = Color.FromArgb("#DDDDDD"),
                CornerRadius = 5,
                BackgroundColor = Colors.Transparent,
                HasShadow = false,
                Padding = new Thickness(3),
                Margin = new Thickness(2)
            };


            Grid innerGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };


            Frame dayNumberFrame = new Frame
            {
                CornerRadius = 20,
                Padding = 0,
                Margin = new Thickness(2),
                HeightRequest = 36,
                WidthRequest = 36,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };


            if (day.IsToday)
            {
                dayNumberFrame.BackgroundColor = Color.FromArgb("#2196F3"); 
            }
            else if (day.IsCurrentMonth)
            {
                dayNumberFrame.BackgroundColor = Colors.Transparent;
            }
            else
            {
                dayNumberFrame.BackgroundColor = Color.FromArgb("#F5F5F5"); 
            }


            Label dayNumberLabel = new Label
            {
                Text = day.DayNumber.ToString(),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = day.IsCurrentMonth ? FontAttributes.Bold : FontAttributes.None
            };


            if (!day.IsCurrentMonth)
            {
                dayNumberLabel.TextColor = Color.FromArgb("#888888"); 
            }
            else if (day.IsToday)
            {
                dayNumberLabel.TextColor = Colors.White; 
            }
            else
            {
                dayNumberLabel.TextColor = Colors.Black; 
            }

            dayNumberFrame.Content = dayNumberLabel;
            innerGrid.Add(dayNumberFrame, 0, 0);

            if (day.TaskCount > 0)
            {
                Label taskCountLabel = new Label
                {
                    Text = day.TaskCount.ToString(),
                    FontSize = 11,
                    TextColor = Colors.Blue,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontAttributes = FontAttributes.Bold
                };


                if (day.TaskCount > 9)
                {
                    taskCountLabel.Text = "9+";
                    taskCountLabel.TextColor = Color.FromArgb("#FF5722"); 
                }

                innerGrid.Add(taskCountLabel, 0, 1);
            }

            cellFrame.Content = innerGrid;

            TapGestureRecognizer tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (sender, e) => await OnDayCellTapped(day);
            cellFrame.GestureRecognizers.Add(tapGesture);

            return cellFrame;
        }


        private async Task OnDayCellTapped(CalendarDay day)
        {

            var databaseService = new Services.DatabaseService();
            var viewModel = new TaskListViewModel(databaseService);
            viewModel.SelectedDate = day.Date;

            var taskListPage = new TaskListPage(viewModel);


            await Navigation.PushAsync(taskListPage);

            // string monthStatus = day.IsCurrentMonth ? "текущего месяца" : "другого месяца";
            // string todayStatus = day.IsToday ? " (сегодня)" : "";
            // 
            // await DisplayAlert(
            //     "Информация о дне",
            //     $"Дата: {day.Date:dd.MM.yyyy}\n" +
            //     $"День: {day.DayNumber}\n" +
            //     $"Месяц: {monthStatus}{todayStatus}\n" +
            //     $"Количество задач: {day.TaskCount}",
            //     "OK"
            // );
        }

        private void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            InitializeCalendar();
        }

        private void OnNextMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            InitializeCalendar();
        }

        private void OnTodayClicked(object sender, EventArgs e)
        {
            _currentDate = DateTime.Now;
            InitializeCalendar();
        }

        private class CalendarDay
        {
            public DateTime Date { get; set; }
            public int DayNumber { get; set; }
            public bool IsCurrentMonth { get; set; }
            public bool IsToday { get; set; }
            public int TaskCount { get; set; }
        }
    }
}