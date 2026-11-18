namespace QuietSpot;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    async void OnCreateClicked(object sender, EventArgs e)
    {
        string fullName = fullNameEntry.Text ?? "";
        string username = usernameEntry.Text ?? "";
        string password = passwordEntry.Text ?? "";
        string confirm = confirmEntry.Text ?? "";

        // Check everything before saving
        if (fullName.Trim() == "" || username.Trim() == "" || password == "" || confirm == "")
        {
            ShowError("Please fill in all the fields.");
            return;
        }

        if (username.Trim().Length < 3)
        {
            ShowError("Username must be at least 3 characters.");
            return;
        }

        if (username.Contains(" "))
        {
            ShowError("Username cannot have spaces.");
            return;
        }

        if (password.Length < 4)
        {
            ShowError("Password must be at least 4 characters.");
            return;
        }

        if (password != confirm)
        {
            ShowError("The two passwords do not match.");
            return;
        }

        // Save the new user (returns an error message if the username is taken)
        string error = await DatabaseService.Instance.RegisterAsync(fullName, username, password);
        if (error != "")
        {
            ShowError(error);
            return;
        }

        await DisplayAlertAsync("Account created", "You can login now.", "OK");
        await Shell.Current.GoToAsync("..");
    }

    async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.IsVisible = true;
    }
}
