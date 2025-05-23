using System.ComponentModel.DataAnnotations;
using src.Models;
using src.Services;


namespace src.Views;

public partial class EditTerm : ContentPage
{
    private Terms _selectedTerm;
	public EditTerm(Terms selectedTerm)
	{
		InitializeComponent();
		_selectedTerm = selectedTerm;
        Title = $"Edit {_selectedTerm.Name}";
        LoadFields();
    }

    private void LoadFields()
    {
        TermNameEntry.Text = _selectedTerm.Name;
        StartDatePicker.Date = _selectedTerm.StartDate;
        EndDatePicker.Date = _selectedTerm.EndDate;
    }

    private async void Save_OnClicked(object sender, EventArgs e)
    {
        string termName = TermNameEntry.Text?.Trim() ?? "";
        DateTime startDate = StartDatePicker.Date;
        DateTime endDate = EndDatePicker.Date;

        string validationMessage = InputValidator.ValidateTerm(termName, startDate, endDate);

        if (validationMessage != null)
        {
            await DisplayAlert("Validation Error", validationMessage, "OK");
            return;
        }

        await DatabaseService.UpdateTerm(
            _selectedTerm.Id,
            termName,
            startDate,
            endDate);

        _selectedTerm.Name = termName;
        _selectedTerm.StartDate = startDate;
        _selectedTerm.EndDate = endDate;

        await DisplayAlert("Success", "Term updated successfully.", "OK");
        await Navigation.PopAsync();
    }

    private async void Cancel_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}