namespace QuietSpot;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // Clear the old text every time the login page is shown
    protected override void OnAppearing()
    {
        base.OnAppearing();
        usernameEntry.Text = "";
        passwordEntry.Text = "";
        showPasswordCheck.IsChecked = false;
        errorLabel.IsVisible = false;
    }

    // Show or hide the password
    void OnShowPasswordChanged(object sender, CheckedChangedEventArgs e)
    {
        passwordEntry.IsPassword = !e.Value;
    }

    async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = usernameEntry.Text ?? "";
        string password = passwordEntry.Text ?? "";

        // 1) check that both fields are filled in
        if (username.Trim() == "" || password == "")
        {
            ShowError("Please enter your username and password.");
            return;
        }

        // 2) check the username and the password in the database
        User user = await DatabaseService.Instance.LoginAsync(username, password);
        if (user == null)
        {
            ShowError("Wrong username or password.");
            return;
        }

        // 3) remember who logged in, show the Approvals tab only for the admin, and go to Home
        Session.CurrentUser = user;
        ((AppShell)Shell.Current).UpdateTabs();
        await Shell.Current.GoToAsync("//HomePage");
    }

    async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.IsVisible = true;
    }
}
