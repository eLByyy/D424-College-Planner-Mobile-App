using Microsoft.Maui.Controls;
using src.Views;
using src.Services;


namespace src
{
    public partial class App : Application
    {
        private bool isFirstLaunch = true;
        public App()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "SqLiteDb.db3");
            DatabaseService.Initialize(dbPath);
            
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginPage = new LoginPage();
            var navPage = new NavigationPage(loginPage)
            {
                BarTextColor = Colors.Black
            };

            return new Window(navPage);
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (isFirstLaunch)
            {
                isFirstLaunch = false;
                _ = CheckForDueNotifications();
            }
        }
        private async Task CheckForDueNotifications()
        {
            var now = DateTime.Now.Date;

            var allCourses = await DatabaseService.GetAllCourses();

            foreach (var course in allCourses)
            {
                if (course.StartNotification)
                {
                    var startNotifyDate = course.StartDate.Date.AddDays(-1);

                    if (startNotifyDate == now)
                    {
                        await Application.Current.MainPage.DisplayAlert("Reminder",
                            $"Your course '{course.Name}' starts tomorrow!", "OK");
                    }
                }

                if (course.StartNotification)
                {
                    var endNotifyDate = course.EndDate.Date.AddDays(-1);
                    if (endNotifyDate == DateTime.Now.Date)
                    {
                        await Application.Current.MainPage.DisplayAlert("Reminder",
                            $"Your course '{course.Name}' ends tomorrow!", "OK");
                    }
                }
                
                if (course.AssessmentNotification)
                {
                    var assessments = await DatabaseService.GetAssessmentsForCourse(course.Id);

                    foreach (var assessment in assessments)
                    {
                        if (assessment.StartDate.Date.AddDays(-1) == now)
                        {
                            await Application.Current.MainPage.DisplayAlert("Reminder", $"Your assessment '{assessment.Name} ' starts tomorrow!", "OK");
                        }

                        if (assessment.EndDate.Date.AddDays(-1) == now)
                        {
                            await Application.Current.MainPage.DisplayAlert("Reminder",
                                $"Your assessment '{assessment.Name}' is due tomorrow!", "OK");
                        }
                    }
                }
            }
        }
    }
}