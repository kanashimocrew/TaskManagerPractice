using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using TaskManager.Services;
using TaskManager.ViewModels;

namespace TaskManager.Views
{
    public partial class CalendarPage : ContentPage
    {
        private DateTime _currentDate;
        private List<CalendarDay> _calendarDays = new List<CalendarDay>();
        private readonly IDatabaseService _databaseService;

        public CalendarPage()
        {
            InitializeComponent();
            _currentDate = DateTime.Now;
            _databaseService = new DatabaseService();

            PrevMonthButton.Clicked += OnPrevMonthClicked;
            NextMonthButton.Clicked += OnNextMonthClicked;
            TodayButton.Clicked += OnTodayClicked;

            CalendarCollectionView.ItemTemplate = CreateDataTemplate();

            InitializeCalendar();
        }

        private DataTemplate CreateDataTemplate()
        {
            return new DataTemplate(() =>
            {
                var mainGrid = new Grid
                {
                    HeightRequest = 70,
                    Margin = new Thickness(1)
                };

                var backgroundFrame = new Frame
                {
                    BorderColor = Color.FromArgb("#DDDDDD"),
                    CornerRadius = 5,
                    BackgroundColor = Colors.Transparent,
                    HasShadow = false,
                    Padding = new Thickness(0),
                    Margin = new Thickness(0)
                };

               //backgroundFrame.SetBinding(Frame.BackgroundColorProperty, "IsToday", converter: new Converters.BoolToColorConverter());

                var contentGrid = new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Star },
                        new RowDefinition { Height = GridLength.Auto }
                    }
                };

                var dayNumberLabel = new Label
                {
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                dayNumberLabel.SetBinding(Label.TextProperty, "DayNumber");
                dayNumberLabel.SetBinding(Label.TextColorProperty, "IsCurrentMonth",
                    converter: new Converters.MonthToTextColorConverter());

                contentGrid.Add(dayNumberLabel, 0, 0);

                var taskCountLabel = new Label
                {
                    FontSize = 11,
                    TextColor = Colors.Blue,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 0, 5),
                    FontAttributes = FontAttributes.Bold
                };
                taskCountLabel.SetBinding(Label.TextProperty, "TaskCount");
                taskCountLabel.SetBinding(Label.IsVisibleProperty, "TaskCount",
                    converter: new Converters.IntToVisibilityConverter());

                contentGrid.Add(taskCountLabel, 0, 1);

                backgroundFrame.Content = contentGrid;
                mainGrid.Children.Add(backgroundFrame);

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += async (sender, e) =>
                {
                    if (mainGrid.BindingContext is CalendarDay day)
                    {
                        await OnDayCellTapped(day);
                    }
                };

                mainGrid.GestureRecognizers.Add(tapGesture);

                backgroundFrame.GestureRecognizers.Add(tapGesture);

                return mainGrid;
            });
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
            try
            {
                _calendarDays.Clear();

                int year = _currentDate.Year;
                int month = _currentDate.Month;

                var taskCounts = await _databaseService.GetTaskCountsForMonthAsync(year, month);

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
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные календаря: {ex.Message}", "OK");
            }
        }

        private void RenderCalendar()
        {
            CalendarCollectionView.ItemsSource = _calendarDays;
        }

        private async Task OnDayCellTapped(CalendarDay day)
        {
            try
            {
                var taskListPage = new TaskListPage(day.Date);
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