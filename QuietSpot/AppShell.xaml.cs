namespace QuietSpot;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // These pages are not in the tab bar, so we register their routes here
        Routing.RegisterRoute(nameof(SpotDetailsPage), typeof(SpotDetailsPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
    }

    // Show the Approvals tab only if the logged in user is the admin
    public void UpdateTabs()
    {
        approvalsTab.IsVisible = Session.IsAdmin;
        usersTab.IsVisible = Session.IsAdmin;
    }
}

