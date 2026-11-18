namespace QuietSpot;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // Runs every time the page is shown, so the numbers are always updated
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // If nobody is logged in, go back to the login page
        if (!Session.IsLoggedIn)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        welcomeLabel.Text = "Welcome, " + Session.CurrentUser.FullName + " (" + Session.CurrentUser.Role + ")";

        var spots = await DatabaseService.Instance.GetSpotsAsync();
        var saved = await DatabaseService.Instance.GetSavedSpotsAsync(Session.CurrentUser.Id);

        totalLabel.Text = spots.Count.ToString();
        savedLabel.Text = saved.Count.ToString();

        // The admin can see the number of users
        if (Session.IsAdmin)
        {
            var users = await DatabaseService.Instance.GetUsersAsync();
            welcomeLabel.Text += "  -  " + users.Count + " users";
        }

        // Preferences = small data saved on the phone (we saved the last viewed spot name)
        string lastViewed = Preferences.Default.Get("last_viewed", "none yet");
        lastViewedLabel.Text = "Last viewed: " + lastViewed;

        // Recommended = quiet and available
        recommendedList.ItemsSource = spots
            .Where(s => s.NoiseLevel == "Quiet" && s.Availability == "Available")
            .ToList();
    }

    // Go to the Spots tab
    async void OnFindClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SpotsPage");
    }

    // Go to the Add tab
    async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//AddSpotPage");
    }

    // Forget the user, hide the Approvals tab and go back to the login page
    async void OnLogoutClicked(object sender, EventArgs e)
    {
        Session.CurrentUser = null;
        ((AppShell)Shell.Current).UpdateTabs();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}

