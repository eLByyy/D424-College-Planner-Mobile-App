using src.Models;
using src.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace src.Views;

public partial class CourseSearchResultsModal : ContentPage
{
	public CourseSearchResultsModal(List<Courses> courses, string searchText)
	{
		InitializeComponent();
        SearchTitlelabel.Text = $"Results for \"{searchText}\"";
        LoadResults(courses);
	}

	private async void LoadResults(List<Courses> courses)
    {
        TimestampLabel.Text = $"Report Generated: {DateTime.Now:G}";

        var allTerms = await DatabaseService.GetTerms();
        int row = 1;
        foreach (var course in courses)
        {
            var term = allTerms.FirstOrDefault(t => t.Id == course.TermId);
            string termName = term?.Name ?? "Unknown";

            ResultsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            ResultsGrid.Add(new Label { Text = termName }, 0, row);
            ResultsGrid.Add(new Label { Text = course.Name }, 1, row);
            ResultsGrid.Add(new Label { Text = course.StartDate.ToShortDateString() }, 2, row);
            ResultsGrid.Add(new Label { Text = course.EndDate.ToShortDateString() }, 3, row);
            ResultsGrid.Add(new Label { Text = course.InstructorName ?? "N/A" }, 4, row);

            row++;
        }
    }

	private async void OnCloseClicked(object sender, EventArgs e)
	{
		try
    {
        await Navigation.PopModalAsync();
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
    }
	}
}