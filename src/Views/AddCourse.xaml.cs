using src.Services;
using src.Models;

namespace src.Views;

public partial class AddCourse : ContentPage
{
    private Terms _selectedTerm;
    private string _selectedAssessment = string.Empty;
    private int courseId;
    private List<Assessments> _pendingAssessments = new();

    
    
	public AddCourse(Terms selectedTerm)
	{
        InitializeComponent();
        _selectedTerm = selectedTerm;
    }

    private async void Save_OnClicked(object sender, EventArgs e)
    {

        int termId = _selectedTerm.Id;
        string courseName = CourseNameEntry.Text?.Trim() ?? "";
        string? status = StatusPicker.SelectedItem?.ToString();
        DateTime startDate = StartDatePicker.Date;
        DateTime endDate = EndDatePicker.Date;
        string instructorName = InstructorName.Text;
        string instructorPhone = InstructorPhone.Text;
        string instructorEmail = InstructorEmail.Text;
        string notes = NotesEditor.Text?.Trim() ?? "";
        
        string validationMessage = InputValidator.ValidateCourse(courseName, status, startDate, endDate, instructorName, instructorPhone, instructorEmail);

        if (validationMessage != null)
        {
            await DisplayAlert("Validation Error", validationMessage, "OK");
            return;
        }

        courseId = await DatabaseService.AddCourse(termId,courseName, status, startDate, endDate, instructorName, instructorPhone, instructorEmail, notes);

        // Save assessments
        foreach (var assessment in _pendingAssessments)
        {
            await DatabaseService.AddAssessment(courseId, assessment.AssessmentType, assessment.Name,
                assessment.StartDate, assessment.EndDate, assessment.NotificationEnabled);
        }

        // Go back to CourseList page
        await Navigation.PopAsync();
    }

    private async void Cancel_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void AssessmentPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (AssessmentPicker.SelectedItem == null)
            return;

        string type = AssessmentPicker.SelectedItem.ToString();
        if (type == "None")
            return;

        var existingTypes = _pendingAssessments.Select(a => a.AssessmentType);
        string validationMessage = InputValidator.ValidateAssessments(_pendingAssessments, type);


        if (validationMessage != null)
        {
            await DisplayAlert("Assessment Limit Reached", validationMessage, "OK");
            return;
        }

        var modal = new AddAssessmentDetailsModal();
        await Navigation.PushModalAsync(modal);

        var result = await modal.CompletionSource.Task;

        if (result != null)
        {
            var (name, startDate, endDate) = result.Value;
            _pendingAssessments.Add(new Assessments
            {
                AssessmentType = type,
                Name = name,
                StartDate = startDate,
                EndDate = endDate
            });
        }

        AssessmentPicker.SelectedIndex = -1;
    }

}