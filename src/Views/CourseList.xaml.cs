using System.Collections.ObjectModel;
using src.Models;
using src.Services;
using src.Views;
using Button = Microsoft.Maui.Controls.Button;

namespace src.Views;

public partial class CourseList : ContentPage
{
    public ObservableCollection<Courses> Courses { get; set; } = new ObservableCollection<Courses>();
    private Terms _selectedTerm;

    public string TermStartDate => _selectedTerm.StartDate.ToString("MM/dd/yyyy");
    public string TermEndDate => _selectedTerm.EndDate.ToString("MM/dd/yyyy");

    public CourseList(Terms selectedTerm)
	{
		InitializeComponent();
        _selectedTerm = selectedTerm;

        BindingContext = this;

        Title = $"{_selectedTerm.Name} Details View";

        LoadCourses();
        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var updatedTerm = await DatabaseService.GetTermById(_selectedTerm.Id);
        if (updatedTerm != null)
        {
            _selectedTerm = updatedTerm;
            Title = $"{_selectedTerm.Name} Details View";

            BindingContext = null;
            BindingContext = this;  //This forces the UI to refresh the binded start and end date labels.
        }

        await LoadCourses();
    }

    private async Task LoadCourses()
    {
        var courses = await DatabaseService.GetCourses(_selectedTerm.Id);
        Courses.Clear();
        foreach (var course in courses)
        {
            Courses.Add(course);
        }
    }
    
    private async void CourseButton_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Courses selectedCourse)
        {
            await Navigation.PushAsync(new CourseDetails(selectedCourse));
        }
    }

    async void AddCourse_OnClicked(object sender, EventArgs e)
    {
        int courseCount = await DatabaseService.GetCourseCountForTerm(_selectedTerm.Id);

        if (courseCount >= 6)
        {
            await DisplayAlert("Course Limit Reached", "You cannot add more than 6 courses per Term.", "OK");
            return;
        }
        await Navigation.PushAsync(new AddCourse(_selectedTerm));
    }

    private async void EditTerm_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new EditTerm(_selectedTerm));
    }

    private async void RemoveTerm_OnClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Delete",
            "Are you sure you want to delete this term and all related courses?", "Yes", "No");
        if (confirm)
        {
            
            await DatabaseService.RemoveTerm(_selectedTerm.Id);
            await Navigation.PopAsync();
        }
    }
}