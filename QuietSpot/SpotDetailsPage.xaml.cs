namespace QuietSpot;

// The "id" sent from the list page goes into SpotId
[QueryProperty(nameof(SpotId), "id")]
public partial class SpotDetailsPage : ContentPage
{
    StudySpot spot;
    int selectedStars = 0;

    public string SpotId { get; set; }

    public SpotDetailsPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadSpot();

        // Preferences API: remember the last spot opened
        Preferences.Default.Set("last_viewed", spot.Name);

        reviewerLabel.Text = "Reviewing as: " + Session.CurrentUser.FullName;

        // Only the admin can delete a spot.
        // Only the admin or the student who added the spot can change its photo.
        deleteButton.IsVisible = Session.IsAdmin;
        changePhotoButton.IsVisible = Session.IsAdmin || spot.CreatedBy == Session.CurrentUser.Username;

        ShowSpot();
        await LoadReviews();
    }

    // Get the spot from the database and check if the logged in user saved it
    async Task LoadSpot()
    {
        spot = await DatabaseService.Instance.GetSpotAsync(int.Parse(SpotId));
        spot.IsSaved = await DatabaseService.Instance.IsSavedAsync(Session.CurrentUser.Id, spot.Id);
    }

    // The labels use data binding, so we just give the page the spot
    void ShowSpot()
    {
        BindingContext = null;
        BindingContext = spot;
    }

    // Show all the reviews of this spot
    async Task LoadReviews()
    {
        var reviews = await DatabaseService.Instance.GetReviewsAsync(spot.Id);
        BindableLayout.SetItemsSource(reviewsLayout, reviews);
        noReviewsLabel.IsVisible = reviews.Count == 0;
    }

    // Save or remove the spot from the Saved list of the logged in user
    async Task ToggleSave()
    {
        await DatabaseService.Instance.ToggleSavedAsync(Session.CurrentUser.Id, spot.Id);
        await LoadSpot();
        ShowSpot();
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        await ToggleSave();
    }

    // Double tap gesture on the picture
    async void OnImageDoubleTapped(object sender, TappedEventArgs e)
    {
        await ToggleSave();
    }

    // Launcher API: open the Google Maps link
    async void OnLocationTapped(object sender, TappedEventArgs e)
    {
        await Launcher.Default.OpenAsync(spot.MapUrl);
    }

    // Share API: opens the share window (name, floor, status, rating and the map link)
    async void OnShareClicked(object sender, EventArgs e)
    {
        string text = spot.Name + " (" + spot.Location + ")"
                    + "\nNoise: " + spot.NoiseLevel
                    + "\nAvailability: " + spot.Availability
                    + "\n" + spot.StarsText + " " + spot.AverageText + " - " + spot.ReviewsCountText
                    + "\nMap: " + spot.MapUrl;

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Share study spot",
            Text = text
        });
    }

    // FilePicker API: choose a photo for this spot
    async void OnChangePhotoClicked(object sender, EventArgs e)
    {
        string path = await PhotoService.PickPhotoAsync();
        if (path == "")
            return;

        spot.PhotoPath = path;
        await DatabaseService.Instance.SaveSpotAsync(spot);
        ShowSpot();
    }

    // A star was tapped: the ClassId of the button is the number of stars
    void OnStarClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        selectedStars = int.Parse(button.ClassId);
        ShowStars();
    }

    void ShowStars()
    {
        star1.Text = selectedStars >= 1 ? "\u2605" : "\u2606";
        star2.Text = selectedStars >= 2 ? "\u2605" : "\u2606";
        star3.Text = selectedStars >= 3 ? "\u2605" : "\u2606";
        star4.Text = selectedStars >= 4 ? "\u2605" : "\u2606";
        star5.Text = selectedStars >= 5 ? "\u2605" : "\u2606";
    }

    async void OnReviewClicked(object sender, EventArgs e)
    {
        if (selectedStars == 0)
        {
            await DisplayAlertAsync("Missing", "Please tap the stars to choose a rating.", "OK");
            return;
        }

        // Make a new review with the name of the logged in user
        Review review = new Review();
        review.SpotId = spot.Id;
        review.UserId = Session.CurrentUser.Id;
        review.Name = Session.CurrentUser.FullName;
        review.Rating = selectedStars;
        review.Comment = string.IsNullOrWhiteSpace(commentEditor.Text) ? "(no comment)" : commentEditor.Text;
        review.CreatedAt = DateTime.Now;

        // One review per user: if he already reviewed this spot, the old review is updated
        bool updated = await DatabaseService.Instance.AddOrUpdateReviewAsync(review);

        // Clear the form
        selectedStars = 0;
        ShowStars();
        commentEditor.Text = "";

        // The average rating changed, so load the spot and the reviews again
        await LoadSpot();
        ShowSpot();
        await LoadReviews();

        await DisplayAlertAsync("Thank you", updated ? "Your review was updated." : "Your review was saved.", "OK");
    }

    // Change the noise level and the availability now
    async void OnUpdateStatusClicked(object sender, EventArgs e)
    {
        if (statusNoisePicker.SelectedIndex == -1 || statusAvailabilityPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Missing", "Please choose the noise level and the availability.", "OK");
            return;
        }

        spot.NoiseLevel = (string)statusNoisePicker.SelectedItem;
        spot.Availability = (string)statusAvailabilityPicker.SelectedItem;
        spot.LastUpdated = DateTime.Now.ToString("dd MMM yyyy, h:mm tt") + " by " + Session.CurrentUser.FullName;

        await DatabaseService.Instance.SaveSpotAsync(spot);
        ShowSpot();

        await DisplayAlertAsync("Updated", "The status of this spot was updated.", "OK");
    }

    // Only the admin sees this button
    async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool sure = await DisplayAlertAsync("Delete", "Delete this study spot?", "Yes", "No");
        if (sure)
        {
            await DatabaseService.Instance.DeleteSpotAsync(spot);
            await Shell.Current.GoToAsync("..");
        }
    }
}
