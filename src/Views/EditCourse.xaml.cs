using src.Models;
using src.Services;

namespace src.Views;

public partial class EditCourse : ContentPage
{
	private Courses _selectedCourse;
    private string _selectedAssessment = string.Empty;
    public EditCourse(Courses selectedCourse)
	{
		InitializeComponent();
		_selectedCourse = selectedCourse;
        Title = $"Edit {_selectedCourse.Name}";
	}

    protected async override void OnAppearing()
    {
        var assessments = (await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id)).ToList();
        EditAssessmentPicker.ItemsSource = assessments;
        EditAssessmentPicker.ItemDisplayBinding = new Binding("Name");


        base.OnAppearing();
        LoadFields();
    }

	private async void LoadFields()
    {
        _selectedCourse = await DatabaseService.GetCourseById(_selectedCourse.Id);

		CourseNameEntry.Text = _selectedCourse.Name;
		StatusPicker.SelectedItem = _selectedCourse.Status;
		StartDatePicker.Date = _selectedCourse.StartDate;
		EndDatePicker.Date = _selectedCourse.EndDate;
		InstructorName.Text = _selectedCourse.InstructorName;
		InstructorPhone.Text = _selectedCourse.InstructorPhone;
		InstructorEmail.Text = _selectedCourse.InstructorEmail;

        NotesEditor.Text = _selectedCourse.Notes;

        await UpdateRemoveAssessmentPicker();
    }

	private async void Save_OnClicked(object sender, EventArgs e)
	{
        string courseName = CourseNameEntry.Text?.Trim() ?? "";
        string status = StatusPicker.SelectedItem?.ToString() ?? "";
        DateTime startDate = StartDatePicker.Date;
        DateTime endDate = EndDatePicker.Date;
        string instructorName = InstructorName.Text?.Trim() ?? "";
        string instructorPhone = InstructorPhone.Text?.Trim() ?? "";
        string instructorEmail = InstructorEmail.Text?.Trim() ?? "";
        string notes = NotesEditor.Text?.Trim() ?? "";
        
//Begin Validations
        string validationMessage = InputValidator.ValidateCourse(courseName, status, startDate, endDate, instructorName, instructorPhone, instructorEmail);

		if (validationMessage != null)
		{
			await DisplayAlert("Validation Error", validationMessage, "OK");
			return;
		}

		await DatabaseService.UpdateCourse(
			_selectedCourse.Id,
			courseName,
            status,
            startDate,
			endDate,			
			instructorName,
			instructorPhone,
			instructorEmail,
            _selectedCourse.StartNotification,
            notes,
            _selectedCourse.AssessmentNotification
            );

        //Assessment logic
        if (!string.IsNullOrEmpty(_selectedAssessment))
        {
            var existingAssessments = (await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id)).ToList();

            string assessmentValidationMessage = InputValidator.ValidateAssessments(existingAssessments, _selectedAssessment);
            if (assessmentValidationMessage != null)
            {
                await DisplayAlert("Assessment Limit Reached", assessmentValidationMessage, "OK");
                return;
            }

            var modal = new AddAssessmentDetailsModal();
            await Navigation.PushModalAsync(modal);

            var result = await modal.CompletionSource.Task;

            if (result != null)
            {
                var (name, assessmentStartDate, assessmentEndDate) = result.Value;

                await DatabaseService.AddAssessment(_selectedCourse.Id, _selectedAssessment, name, assessmentStartDate,
                    assessmentEndDate, _selectedCourse.AssessmentNotification);
                await UpdateRemoveAssessmentPicker();
            }
        }
        
		await DisplayAlert("Success", "Course updated successfully.", "OK");
		await Navigation.PopAsync();
    }

	private async void Cancel_OnClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

    private async void AssessmentPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (AddAssessmentPicker.SelectedItem == null)
            return;

        string type = AddAssessmentPicker.SelectedItem.ToString();
        if (type == "None")
            return;

        var modal = new AddAssessmentDetailsModal();
        await Navigation.PushModalAsync(modal);

        var result = await modal.CompletionSource.Task;

        if (result != null)
        {
            var (name, startDate, endDate) = result.Value;

            var existingAssessments = await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id);
            string validationMessage = InputValidator.ValidateAssessments((List<Assessments>)existingAssessments, type);

            if (validationMessage != null)
            {
                await DisplayAlert("Assessment Limit Reached", validationMessage, "OK");
                return;
            }

            await DatabaseService.AddAssessment(_selectedCourse.Id, type, name, startDate, endDate, _selectedCourse.AssessmentNotification);
            await UpdateRemoveAssessmentPicker();
        }

        AddAssessmentPicker.SelectedIndex = -1;

    }

    private async void RemoveAssessmentPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (RemoveAssessmentPicker.SelectedIndex == -1) return;

        string selectedAssessment = (string)RemoveAssessmentPicker.SelectedItem;

        RemoveAssessmentPicker.SelectedIndex = -1;

        var assessmentToRemove = await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id);
        var assessment = assessmentToRemove.FirstOrDefault(a => a.AssessmentType == selectedAssessment);

        if (assessment != null)
        {
            bool confirmRemove = await DisplayAlert("Confirm Removal",
                $"Are you sure you want to remove {selectedAssessment}?", "Yes", "No");

            if (confirmRemove)
            {
                await DatabaseService.RemoveAssessment(assessment.Id);
                await UpdateRemoveAssessmentPicker();
            }
        }
    }

    private async Task UpdateRemoveAssessmentPicker()
    {
        var courseAssessments = await DatabaseService.GetAssessmentsForCourse(_selectedCourse.Id);
        RemoveAssessmentPicker.ItemsSource = courseAssessments.Select(a => a.AssessmentType).ToList();
    }

    private async void EditAssessmentPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (EditAssessmentPicker.SelectedItem is Assessments selectedAssessments)
        {
            var editModal = new EditAssessmentDetailsModal(selectedAssessments);
            editModal.AssessmentUpdated += async (selectedAssessments, updatedAssessment) =>
            {
                await DisplayAlert("Success", "Assessment updated!", "OK");

            };

            await Navigation.PushModalAsync(editModal);
        }
    }
}