namespace src.Views;

public partial class AddAssessmentDetailsModal : ContentPage
{
    public TaskCompletionSource<(string name, DateTime start, DateTime end)?> CompletionSource { get; set; } = new();
	public AddAssessmentDetailsModal()
	{
		InitializeComponent();
	}

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var name = AssessmentNameEntry.Text?.Trim();
        var start = StartDatePicker.Date;
        var end = EndDatePicker.Date;

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlert("Missing Info", "Please enter a name.", "OK");
            return;
        }

        if (start == end)
        {
            await DisplayAlert("Invalid Dates", "Start date and end date cannot be the same.", "OK");
            return;
        }

        if (end < start)
        {
            await DisplayAlert("Invalid Dates", "End date cannot be before start date.", "OK");
            return;
        }

        CompletionSource.SetResult((name, start, end));
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        CompletionSource.SetResult(null);
        await Navigation.PopModalAsync();
    }
}