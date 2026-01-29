using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using TaskManager.Services;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class CalendarPage : ContentPage
    {
        private DateTime _currentDate;
        private List<CalendarDay> _calendarDays = new List<CalendarDay>();
        private DatabaseService _databaseService;

        public CalendarPage()
        {
            InitializeComponent();
            _currentDate = DateTime.Now;
            _databaseService = new DatabaseService(); 

            PrevMonthButton.Clicked += OnPrevMonthClicked;
            NextMonthButton.Clicked += OnNextMonthClicked;
            TodayButton.Clicked += OnTodayClicked;

            InitializeCalendar();
        }

        private async 
        Task
InitializeCalendar()
        {
            UpdateMonthYearLabel();
            await LoadCalendarData(); 
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

        private async Task LoadCalendarData()
        {
            _calendarDays.Clear();

            int year = _currentDate.Year;
            int month = _currentDate.Month;

            Dictionary<DateTime, int> taskCounts = await _databaseService.GetTaskCountsForMonthAsync(year, month);

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
            try
            {

                var taskListPage = new TaskListPage();

                if (taskListPage.BindingContext is TaskListViewModel viewModel)
                {
                    viewModel.SelectedDate = day.Date;
                }

                taskListPage.Title = $"Задачи на {day.Date:dd.MM.yyyy}";

                await Navigation.PushAsync(taskListPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось перейти: {ex.Message}", "OK");
            }
        }

        private async void OnPrevMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            await InitializeCalendar(); 
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            await InitializeCalendar(); 
        }

        private async void OnTodayClicked(object sender, EventArgs e)
        {
            _currentDate = DateTime.Now;
            await InitializeCalendar();
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