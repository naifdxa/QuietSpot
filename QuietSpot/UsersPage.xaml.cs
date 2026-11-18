namespace QuietSpot;

public partial class UsersPage : ContentPage
{
    public UsersPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Only the admin can use this page
        if (!Session.IsAdmin)
        {
            notAdminLabel.IsVisible = true;
            summaryGrid.IsVisible = false;
            usersList.IsVisible = false;
            return;
        }

        notAdminLabel.IsVisible = false;
        summaryGrid.IsVisible = true;
        usersList.IsVisible = true;
        await LoadUsers();
    }

    // Load all the users and update the three numbers
    async Task LoadUsers()
    {
        var users = await DatabaseService.Instance.GetUsersAsync();

        totalLabel.Text = users.Count.ToString();
        adminsLabel.Text = users.Count(u => u.Role == "Admin").ToString();
        studentsLabel.Text = users.Count(u => u.Role == "Student").ToString();

        usersList.ItemsSource = users;
    }

    // Student -> Admin, or Admin -> Student
    async void OnChangeRoleClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var user = (User)button.BindingContext;

        // The admin cannot remove his own admin role (so the app is never left without an admin)
        if (user.Id == Session.CurrentUser.Id)
        {
            await DisplayAlertAsync("Not allowed", "You cannot change your own role.", "OK");
            return;
        }

        string newRole = user.Role == "Admin" ? "Student" : "Admin";

        bool sure = await DisplayAlertAsync("Change role",
            "Make " + user.FullName + " a " + newRole + "?", "Yes", "No");
        if (!sure)
            return;

        await DatabaseService.Instance.SetUserRoleAsync(user.Id, newRole);
        await LoadUsers();
    }

    async void OnDeleteUserClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var user = (User)button.BindingContext;

        if (user.Id == Session.CurrentUser.Id)
        {
            await DisplayAlertAsync("Not allowed", "You cannot delete your own account.", "OK");
            return;
        }

        bool sure = await DisplayAlertAsync("Delete user",
            "Delete the account of " + user.FullName + "?", "Yes", "No");
        if (!sure)
            return;

        await DatabaseService.Instance.DeleteUserAsync(user);
        await LoadUsers();
    }
}
