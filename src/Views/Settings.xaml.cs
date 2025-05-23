using src.Services;

namespace src.Views;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();
	}

	private async void OnLoadSampleDataClicked(object sender, EventArgs e)
	{
		await DatabaseService.LoadSampleData();

		await DisplayAlert("Success", "Sample data loaded successfully.", "OK");
		
	}

	private async void OnClearSampleDataClicked(Object sender, EventArgs e)
	{
		bool success = await DatabaseService.ClearSampleData();

		if (!success)
		{
			await DisplayAlert("Error", "There is no sample data to clear.", "OK");
		}
		else
		{
			await DisplayAlert("Success", "Sample data cleared successfully.", "OK");
		}
	}

	private async void OnClearDatabaseClicked(object sender, EventArgs e)
	{
		bool confirm = await DisplayAlert("Confirm", "Warning: Clearing the database will remove all Users, Terms, Courses, and associated data. Are you sure you want to proceed?","YES","NO");
		if (confirm)
		{
			await DatabaseService.ClearDatabase();
			await DisplayAlert("Success", "Database cleared.", "OK");
		}
	}
}