namespace TaskManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var calendarPage = new Views.CalendarPage();
            MainPage = new NavigationPage(calendarPage)
            {
                BarBackgroundColor = Color.FromArgb("#512DA8"),
                BarTextColor = Colors.White
            };
        }
    }
}