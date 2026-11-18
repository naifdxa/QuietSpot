namespace QuietSpot;

public partial class ApprovalsPage : ContentPage
{
    public ApprovalsPage()
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
            pendingList.IsVisible = false;
            return;
        }

        notAdminLabel.IsVisible = false;
        pendingList.IsVisible = true;
        await LoadPending();
    }

    async Task LoadPending()
    {
        pendingList.ItemsSource = await DatabaseService.Instance.GetPendingSpotsAsync();
    }

    // The button is inside each item, so its BindingContext is that spot
    async void OnApproveClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var spot = (StudySpot)button.BindingContext;

        spot.Status = "Approved";
        await DatabaseService.Instance.SaveSpotAsync(spot);
        await LoadPending();
    }

    async void OnRejectClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var spot = (StudySpot)button.BindingContext;

        spot.Status = "Rejected";
        await DatabaseService.Instance.SaveSpotAsync(spot);
        await LoadPending();
    }
}
