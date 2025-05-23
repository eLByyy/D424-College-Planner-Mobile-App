using src.Models;
using System.Threading.Tasks;
using src.Services;
using Plugin.LocalNotification;
using Microsoft.Maui.ApplicationModel;

namespace src.Views;

public partial class CourseDetails : ContentPage
{
    public Courses _selectedCourse;
	public CourseDetails(Courses selectedCourse)
	{
		InitializeComponent();
		_selectedCourse = selectedCourse;
        Title = $"{_selectedCourse.Name} Details";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCourseDetails();
        await CheckForDueNotifications();
    }

    private async Task LoadCourseDetails()
    {
        var updatedCourse = await DatabaseService.GetCourseById(_selectedCourse.Id);

        if (updatedCourse != null)
        {
            _selectedCourse = updatedCourse;
            BindingContext = null;
            BindingContext = _selectedCourse;

            Title = $"{_selectedCourse.Name} Details";

            StatusLabel.Text = _selectedCourse.Status;
            StartDateLabel.Text = _selectedCourse.StartDate.ToString("MMMM dd, yyyy");
            EndDateLabel.Text = _selectedCourse.EndDate.ToString("MMMM dd, yyyy");
            InstructorNameLabel.Text = _selectedCourse.InstructorName;
            InstructorPhoneLabel.Text = _selectedCourse.InstructorPhone;
            InstructorEmailLabel.Text = _selectedCourse.InstructorEmail;

            //Display Notes
            NotesContentLabel.Text = string.IsNullOrEmpty(_selectedCourse.Notes) ? "No notes added." : _selectedCourse.Notes;

            //Set notification toggle state
            NotificationSwitch.IsToggled = _selectedCourse.StartNotification;
            AssessmentToggleAlert.IsToggled = _selectedCourse.AssessmentNotification;

            //Load assessments
            var assessments = await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id);
            AssessmentsStack.Children.Clear();

            if (assessments.Any())
            {
                foreach (var assessment in assessments)
                {
                    var assessmentLayout = new StackLayout
                    {
                        Spacing = 2,
                        Padding = new Thickness(0,0,0,10)
                    };

                    var titleLabel = new Label
                    {
                        Text = $"{assessment.AssessmentType} - {assessment.Name}",
                        FontSize = 14
                    };

                    var startLabel = new Label
                    { 
                        Text = $"Start: {assessment.StartDate:MM/dd/yyyy}",
                        FontSize = 13
                    };

                    var endLabel = new Label
                    {
                        Text = $"End: {assessment.EndDate:MM/dd/yyyy}",
                        FontSize = 13
                    };

                    assessmentLayout.Children.Add(titleLabel);
                    assessmentLayout.Children.Add(startLabel);
                    assessmentLayout.Children.Add(endLabel);
                    AssessmentsStack.Children.Add(assessmentLayout);
                }
            }
        }
    }

    private async Task CheckForDueNotifications()
    {
        var now = DateTime.Now.Date;
        var startNotifyDate = _selectedCourse.StartDate.Date.AddDays(-1);
        if (_selectedCourse.StartNotification && startNotifyDate == now)
        {
            await DisplayAlert("Reminder", $"Your course '{_selectedCourse.Name} starts tomorrow!", "OK");
        }

        var endNotifyDate = _selectedCourse.EndDate.Date.AddDays(-1);
        if (_selectedCourse.StartNotification && endNotifyDate == now)
        {
            await DisplayAlert("Reminder", $"Your course '{_selectedCourse.Name}' ends tomorrow!", "OK");
        }

        if (_selectedCourse.AssessmentNotification)
        {
            var assessments = await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id);
            foreach (var assessment in assessments)
            {
                var assessmentStartNotifyDate = assessment.StartDate.Date.AddDays(-1);
                var assessmentEndNotifyDate = assessment.EndDate.Date.AddDays(-1);

                if (assessmentStartNotifyDate == now)
                {
                    await DisplayAlert("Reminder", $"Your assessment '{assessment.Name}' starts tomorrow!", "OK");
                }

                if (assessmentEndNotifyDate == now)
                {
                    await DisplayAlert("Reminder", $"Your assessment '{assessment.Name}' is due tomorrow!", "OK");
                }
            }
        }
    }

    private async void RemoveCourse_OnClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Delete",
            $"Are you sure you want to remove the course '{_selectedCourse.Name}'?", "Yes", "No");
        if (confirm)
        {
            await CancelNotifications();
            await DatabaseService.RemoveCourse(_selectedCourse.Id);
            await Navigation.PopAsync();
        }
    }

    private async void EditCourse_OnClicked(object sender, EventArgs e)
    {
        if (_selectedCourse != null)
        {
            await Navigation.PushAsync(new EditCourse(_selectedCourse));
        }
    }

    private async void ShareNotes_OnClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_selectedCourse.Notes))
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Share Course Notes",
                Text = _selectedCourse.Notes,
                Subject = $"Course Notes For {_selectedCourse.Name}"
            });
        }
        else
        {
            await DisplayAlert("No Notes", "There are no notes to share.", "OK");
        }
    }

    private async void NotificationSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (e.Value)
        {
            await ScheduleNotifications();
        }
        else
        {
            CancelNotifications();
    }
        _selectedCourse.StartNotification = e.Value;
        await DatabaseService.UpdateCourse(_selectedCourse.Id, _selectedCourse.Name, _selectedCourse.Status,
    _selectedCourse.StartDate, _selectedCourse.EndDate, _selectedCourse.InstructorName,
    _selectedCourse.InstructorPhone, _selectedCourse.InstructorEmail, _selectedCourse.StartNotification,
    _selectedCourse.Notes, _selectedCourse.AssessmentNotification);
    }

    private async Task ScheduleNotifications()
    {
        var startNotificationId = _selectedCourse.Id * 10 + 1;
        var endNotificationId = _selectedCourse.Id * 10 + 2;

        if (_selectedCourse.StartDate > DateTime.Now.AddDays(1))
        {
            var startNotification = new NotificationRequest
            {
                NotificationId = startNotificationId,
                Title = "Upcoming Course Start",
                Description = $"Your course '{_selectedCourse.Name}' starts tomorrow!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.SpecifyKind(_selectedCourse.StartDate.AddDays(-1), DateTimeKind.Local)
                }
            };
            await LocalNotificationCenter.Current.Show(startNotification);
            _selectedCourse.StartNotificationId = startNotificationId;
        }

        if (_selectedCourse.EndDate > DateTime.Now.AddDays(1))
        {
            var endNotification = new NotificationRequest
            {
                NotificationId = endNotificationId,
                Title = "Upcoming Course End",
                Description = $"Your course '{_selectedCourse.Name}' ends tomorrow!",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = DateTime.SpecifyKind(_selectedCourse.EndDate.AddDays(-1), DateTimeKind.Local)
                }
            };
            await LocalNotificationCenter.Current.Show(endNotification);
            _selectedCourse.EndNotificationId = endNotificationId;
        }

        await DatabaseService.UpdateCourse(_selectedCourse.Id, _selectedCourse.Name, _selectedCourse.Status,
    _selectedCourse.StartDate, _selectedCourse.EndDate, _selectedCourse.InstructorName,
    _selectedCourse.InstructorPhone, _selectedCourse.InstructorEmail, _selectedCourse.StartNotification,
    _selectedCourse.Notes, _selectedCourse.AssessmentNotification);
    }

    private async Task CancelNotifications()
    {
        if (_selectedCourse.StartNotificationId.HasValue)
        {
            LocalNotificationCenter.Current.Cancel(_selectedCourse.StartNotificationId.Value);
            _selectedCourse.StartNotificationId = null;
        }
        if (_selectedCourse.EndNotificationId.HasValue)
        {
            LocalNotificationCenter.Current.Cancel(_selectedCourse.EndNotificationId.Value);
            _selectedCourse.EndNotificationId = null;
        }

        await DatabaseService.UpdateCourse(_selectedCourse.Id, _selectedCourse.Name, _selectedCourse.Status,
    _selectedCourse.StartDate, _selectedCourse.EndDate, _selectedCourse.InstructorName,
    _selectedCourse.InstructorPhone, _selectedCourse.InstructorEmail, _selectedCourse.StartNotification,
    _selectedCourse.Notes, _selectedCourse.AssessmentNotification);
    }

    private async void AssessmentToggleAlert_Toggled (object sender, ToggledEventArgs e)
    {
        bool isToggledOn = e.Value;
        _selectedCourse.AssessmentNotification = isToggledOn;

        await DatabaseService.UpdateCourse(
            _selectedCourse.Id,
            _selectedCourse.Name,
            _selectedCourse.Status,
            _selectedCourse.StartDate,
            _selectedCourse.EndDate,
            _selectedCourse.InstructorName,
            _selectedCourse.InstructorPhone,
            _selectedCourse.InstructorEmail,
            _selectedCourse.StartNotification,
            _selectedCourse.Notes,
            _selectedCourse.AssessmentNotification
            );
    }
}