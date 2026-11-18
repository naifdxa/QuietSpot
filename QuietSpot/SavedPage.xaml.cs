namespace QuietSpot;

public partial class SavedPage : ContentPage
{
    public SavedPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSaved();
    }

    // Only the spots saved by the logged in user
    async Task LoadSaved()
    {
        savedList.ItemsSource = await DatabaseService.Instance.GetSavedSpotsAsync(Session.CurrentUser.Id);
    }

    async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var spot = (StudySpot)button.BindingContext;
        await Shell.Current.GoToAsync($"{nameof(SpotDetailsPage)}?id={spot.Id}");
    }

    // Remove from saved (the spot itself is not deleted)
    async void OnRemoveClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var spot = (StudySpot)button.BindingContext;

        await DatabaseService.Instance.ToggleSavedAsync(Session.CurrentUser.Id, spot.Id);
        await LoadSaved();
    }
}
