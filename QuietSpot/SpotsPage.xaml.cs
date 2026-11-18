namespace QuietSpot;

public partial class SpotsPage : ContentPage
{
    public SpotsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSpots();
    }

    // Get all spots from the database, then apply the search and the filters
    async Task LoadSpots()
    {
        var spots = await DatabaseService.Instance.GetSpotsAsync();

        string search = searchBar.Text;
        string noise = noisePicker.SelectedItem as string;
        string availability = availabilityPicker.SelectedItem as string;

        // Search by name or location
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            spots = spots.Where(s => s.Name.ToLower().Contains(search)
                                  || s.Location.ToLower().Contains(search)).ToList();
        }

        // Filter by noise level
        if (noise != null && noise != "All")
            spots = spots.Where(s => s.NoiseLevel == noise).ToList();

        // Filter by availability
        if (availability != null && availability != "All")
            spots = spots.Where(s => s.Availability == availability).ToList();

        spotsList.ItemsSource = spots;
    }

    async void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        await LoadSpots();
    }

    async void OnFilterChanged(object sender, EventArgs e)
    {
        await LoadSpots();
    }

    async void OnClearClicked(object sender, EventArgs e)
    {
        searchBar.Text = "";
        noisePicker.SelectedIndex = -1;
        availabilityPicker.SelectedIndex = -1;
        await LoadSpots();
    }

    // The button is inside each item, so its BindingContext is that spot
    async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var spot = (StudySpot)button.BindingContext;
        await Shell.Current.GoToAsync($"{nameof(SpotDetailsPage)}?id={spot.Id}");
    }

    // Tap gesture: tapping anywhere on the card opens the details page
    async void OnCardTapped(object sender, TappedEventArgs e)
    {
        var item = (BindableObject)sender;
        var spot = (StudySpot)item.BindingContext;
        await Shell.Current.GoToAsync($"{nameof(SpotDetailsPage)}?id={spot.Id}");
    }
}
