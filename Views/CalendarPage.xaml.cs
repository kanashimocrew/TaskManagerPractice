using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

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

            // Назначаем обработчики событий
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
            // Русские названия месяцев
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

            // Тестовые данные для задач (в реальном приложении будет из базы данных)
            Dictionary<DateTime, int> taskCounts = GenerateTestTaskCounts(year, month);

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Начинаем календарь с понедельника
            int firstDayOffset = ((int)firstDayOfMonth.DayOfWeek - 1 + 7) % 7;

            // 1. Дни предыдущего месяца
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

            // 2. Дни текущего месяца
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

            // 3. Дни следующего месяца (чтобы заполнить 6 недель)
            int totalCells = 42; // 6 недель × 7 дней
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

            // Добавляем задачи для некоторых дней
            for (int i = 0; i < 15; i++)
            {
                int randomDay = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                DateTime date = new DateTime(year, month, randomDay);
                counts[date] = random.Next(1, 6); // от 1 до 5 задач
            }

            // Обязательно добавляем задачи на сегодня
            if (year == DateTime.Now.Year && month == DateTime.Now.Month)
            {
                counts[DateTime.Now.Date] = random.Next(1, 4);
            }

            // Добавляем несколько дней с большим количеством задач для демонстрации
            counts[new DateTime(year, month, 5)] = 8;
            counts[new DateTime(year, month, 15)] = 12;
            counts[new DateTime(year, month, 25)] = 5;

            return counts;
        }

        private void RenderCalendar()
        {
            // Очищаем предыдущий календарь
            CalendarGrid.Children.Clear();
            CalendarGrid.RowDefinitions.Clear();
            CalendarGrid.ColumnDefinitions.Clear();

            // Создаем 6 строк (недель) и 7 колонок (дней недели)
            for (int row = 0; row < 6; row++)
            {
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = 70 });
            }

            for (int col = 0; col < 7; col++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }

            // Заполняем календарь
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
            // Основная рамка ячейки
            Frame cellFrame = new Frame
            {
                BorderColor = Color.FromArgb("#DDDDDD"),
                CornerRadius = 5,
                BackgroundColor = Colors.Transparent,
                HasShadow = false,
                Padding = new Thickness(3),
                Margin = new Thickness(2)
            };

            // Внутренний контейнер
            Grid innerGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Круг с числом дня
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

            // Цвет фона зависит от того, сегодня ли это и текущий ли это месяц
            if (day.IsToday)
            {
                dayNumberFrame.BackgroundColor = Color.FromArgb("#2196F3"); // Синий для сегодняшнего дня
            }
            else if (day.IsCurrentMonth)
            {
                dayNumberFrame.BackgroundColor = Colors.Transparent;
            }
            else
            {
                dayNumberFrame.BackgroundColor = Color.FromArgb("#F5F5F5"); // Светло-серый для дней других месяцев
            }

            // Число дня
            Label dayNumberLabel = new Label
            {
                Text = day.DayNumber.ToString(),
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = day.IsCurrentMonth ? FontAttributes.Bold : FontAttributes.None
            };

            // Цвет текста
            if (!day.IsCurrentMonth)
            {
                dayNumberLabel.TextColor = Color.FromArgb("#888888"); // Серый для дней других месяцев
            }
            else if (day.IsToday)
            {
                dayNumberLabel.TextColor = Colors.White; // Белый для сегодняшнего дня
            }
            else
            {
                dayNumberLabel.TextColor = Colors.Black; // Черный для остальных дней текущего месяца
            }

            dayNumberFrame.Content = dayNumberLabel;
            innerGrid.Add(dayNumberFrame, 0, 0);

            // Отображение количества задач
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

                // Для большого количества задач показываем "9+"
                if (day.TaskCount > 9)
                {
                    taskCountLabel.Text = "9+";
                    taskCountLabel.TextColor = Color.FromArgb("#FF5722"); // Оранжевый
                }

                innerGrid.Add(taskCountLabel, 0, 1);
            }

            cellFrame.Content = innerGrid;

            // Обработчик нажатия на ячейку
            TapGestureRecognizer tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (sender, e) => OnDayCellTapped(day);
            cellFrame.GestureRecognizers.Add(tapGesture);

            return cellFrame;
        }

        private async void OnDayCellTapped(CalendarDay day)
        {
            string monthStatus = day.IsCurrentMonth ? "текущего месяца" : "другого месяца";
            string todayStatus = day.IsToday ? " (сегодня)" : "";

            await DisplayAlert(
                "Информация о дне",
                $"Дата: {day.Date:dd.MM.yyyy}\n" +
                $"День: {day.DayNumber}\n" +
                $"Месяц: {monthStatus}{todayStatus}\n" +
                $"Количество задач: {day.TaskCount}",
                "OK"
            );
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

        // Вспомогательный класс для хранения данных о дне
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