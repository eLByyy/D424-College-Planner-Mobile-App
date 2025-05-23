using src.Models;
using src.Services;

namespace src.Views;

public partial class EditAssessmentDetailsModal : ContentPage
{
    private readonly Assessments _assessments;

    public event EventHandler<Assessments> AssessmentUpdated;
    public TaskCompletionSource<(string name, DateTime start, DateTime end)?> CompletionSource { get; set; } = new();

    public EditAssessmentDetailsModal(Assessments assessments)
    {
        InitializeComponent();
        _assessments = assessments;
        BindingContext = _assessments;

        AssessmentNameEntry.Text = _assessments.Name;
        StartDatePicker.Date = _assessments.StartDate;
        EndDatePicker.Date = _assessments.EndDate;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        _assessments.Name = AssessmentNameEntry.Text;
        _assessments.StartDate = StartDatePicker.Date;
        _assessments.EndDate = EndDatePicker.Date;

        if (string.IsNullOrEmpty(_assessments.Name))
        {
            await DisplayAlert("Missing Info", "Please enter a name.", "OK");
            return;
        }

        if (_assessments.StartDate == _assessments.EndDate)
        {
            await DisplayAlert("Invalid Dates", "Start date and end date cannot be the same.", "OK");
            return;
        }

        if (_assessments.EndDate < _assessments.StartDate)
        {
            await DisplayAlert("Invalid Dates", "End date cannot be before start date.", "OK");
            return;
        }

        await DatabaseService.UpdateAssessment(_assessments);
        AssessmentUpdated?.Invoke(this,_assessments);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        CompletionSource.SetResult(null);
        await Navigation.PopModalAsync();
    }
}