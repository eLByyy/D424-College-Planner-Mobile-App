using System.Collections.ObjectModel;
using src.Services;
using src.Models;
using System.Threading.Tasks;

//This is the Landing Page of the App

namespace src.Views
{
    public partial class TermsList : ContentPage
    {
        public ObservableCollection<Terms> Terms { get; set; } = new();
        private Terms _selectedTerm;
        private List<Courses> allCourses = new();

        public TermsList()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void TermButton_OnClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is Terms selectedTerm)
            {
                await Navigation.PushAsync(new CourseList(selectedTerm));
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //Load sample data only if no data already exists (no terms)

            //Removing LoadSampleData feature for non-demo release of app
            //var terms = await DatabaseService.GetTerms();
            //if (!terms.Any())
            //{
            //    await DatabaseService.LoadSampleData();
            //    terms = await DatabaseService.GetTerms();
            //}

            allCourses = await DatabaseService.GetAllCourses();

            await LoadTerms();
        }

        private async Task LoadTerms()
        {
            Terms.Clear();
            var terms = await DatabaseService.GetTerms();
            foreach (var term in terms)
            {
                Terms.Add(term);
            } 
        }

        async void AddTerm_OnClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddTerm());
        }

        private async void ClearDatabase_OnClicked(object? sender, EventArgs e)
        {
            await Services.DatabaseService.ClearDatabase();
            
            await DisplayAlert("Success", "All data has been cleared.", "OK");

            await LoadTerms();  
           
        }

        private async void LoadSampleData_OnClicked(object? sender, EventArgs e)
        {
            await DatabaseService.LoadSampleData();

            await LoadTerms();

            await DisplayAlert("Sample Data", "Sample Data loaded.", "OK");
        }      

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirmLogout = await Application.Current.MainPage.DisplayAlert("Confirm Logout", "Are you sure you want to log out?", "Yes", "No");
            
            if (confirmLogout)
            {
                Session.CurrentUser = null;
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        private bool isSearching = false;
        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            if (isSearching) return;
            isSearching = true;

            try
            {
                string searchText = CourseSearchBar.Text?.ToLower() ?? "";

                if (string.IsNullOrEmpty(searchText))
                {
                    return;
                }

                var filtered = allCourses
                    .Where(c =>
                        (c.Name?.ToLower().Contains(searchText) ?? false) ||
                        (c.InstructorName?.ToLower().Contains(searchText) ?? false))
                    .ToList();

                if (filtered.Any())
                {
                    var modal = new CourseSearchResultsModal(filtered, searchText);
                    await Navigation.PushModalAsync(modal);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("No Results", "No matching course info found.", "OK");
                }
            }
            finally
            {
                isSearching = false;
            }                      
        }
    }
}
