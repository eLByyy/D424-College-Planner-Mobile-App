using System;
using Microsoft.Maui.Controls;
using src.Services;

namespace src.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private async void OnLoginClicked(object sender, EventArgs e)
    {
		string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text;

		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validation Error", "Please enter both username and password.", "OK");
            return;
        }

        bool isValid = await DatabaseService.ValidateUser(username, password);

        if (isValid)
        {
            var user = await DatabaseService.GetUserByUsername(username); 

            if (user != null)
            {
                Session.Login(user); // Sets Session.CurrentUser
                Application.Current.MainPage = new NavigationPage(new TermsList())
                {
                    BarTextColor = Colors.Black
                };
            }
            else
            {
                await DisplayAlert("Error", "Unexpected error: user not found after validation.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid username or password.", "OK");
        }
    }

    private async void OnCreateAccountClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CreateAccountPage());
    }

    private async void Settings_OnClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new Settings());
    }
}