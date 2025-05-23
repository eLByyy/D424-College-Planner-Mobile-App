using src.Models;
using src.Services;

namespace src.Views
{
    public partial class AddTerm : ContentPage
    {
        public AddTerm()
        {
            InitializeComponent();

        }

        private async void Save_OnClicked(object sender, EventArgs e)
        {
            string termName = TermNameEntry.Text;
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;

            string validationMessage = InputValidator.ValidateTerm(termName, startDate, endDate);

            if (validationMessage != null)
            {
                await DisplayAlert("Validation Error", validationMessage, "OK");
                return;
            }

            await DatabaseService.AddTerm(termName, startDate, endDate);

            await DisplayAlert("Success", "Term created successfully.", "OK");
            await Navigation.PopAsync();
        }

        private async void Cancel_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}