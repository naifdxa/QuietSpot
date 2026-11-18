namespace QuietSpot;

public partial class AddSpotPage : ContentPage
{
    string chosenPhotoPath = "";

    public AddSpotPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Session.IsAdmin)
            infoLabel.Text = "You are the admin, so your spots are approved right away.";
        else
            infoLabel.Text = "New spots are checked by the admin before other students can see them.";

        await LoadSubmissions();
    }

    // Show the spots added by the logged in user, with their status
    async Task LoadSubmissions()
    {
        var mine = await DatabaseService.Instance.GetMySpotsAsync(Session.CurrentUser.Username);
        BindableLayout.SetItemsSource(submissionsLayout, mine);
        noSubmissionsLabel.IsVisible = mine.Count == 0;
    }

    // FilePicker API: choose a photo and show a small preview
    async void OnChoosePhotoClicked(object sender, EventArgs e)
    {
        string path = await PhotoService.PickPhotoAsync();
        if (path != "")
        {
            chosenPhotoPath = path;
            photoPreview.Source = ImageSource.FromFile(path);
            photoPreview.IsVisible = true;
        }
    }

    async void OnSaveClicked(object sender, EventArgs e)
    {
        // Check that everything is filled in
        if (string.IsNullOrWhiteSpace(nameEntry.Text) || string.IsNullOrWhiteSpace(locationEntry.Text))
        {
            await DisplayAlertAsync("Missing", "Please enter the name and the location.", "OK");
            return;
        }

        if (noisePicker.SelectedIndex == -1 || availabilityPicker.SelectedIndex == -1)
        {
            await DisplayAlertAsync("Missing", "Please choose the noise level and the availability.", "OK");
            return;
        }

        // Make a new spot and save it to the database
        StudySpot spot = new StudySpot();
        spot.Name = nameEntry.Text;
        spot.Location = locationEntry.Text;
        spot.MapLink = mapEntry.Text ?? "";
        spot.OpeningHours = string.IsNullOrWhiteSpace(hoursEntry.Text) ? "Not specified" : hoursEntry.Text;
        spot.NoiseLevel = (string)noisePicker.SelectedItem;
        spot.Availability = (string)availabilityPicker.SelectedItem;
        spot.LastUpdated = DateTime.Now.ToString("dd MMM yyyy, h:mm tt");
        spot.Description = descriptionEditor.Text ?? "";
        spot.PhotoPath = chosenPhotoPath;
        spot.CreatedBy = Session.CurrentUser.Username;

        // The admin's spots are approved right away, the students' spots wait for the admin
        spot.Status = Session.IsAdmin ? "Approved" : "Pending";

        await DatabaseService.Instance.SaveSpotAsync(spot);

        // Clear the form
        nameEntry.Text = "";
        locationEntry.Text = "";
        mapEntry.Text = "";
        hoursEntry.Text = "";
        noisePicker.SelectedIndex = -1;
        availabilityPicker.SelectedIndex = -1;
        descriptionEditor.Text = "";
        chosenPhotoPath = "";
        photoPreview.IsVisible = false;

        if (Session.IsAdmin)
        {
            await DisplayAlertAsync("Saved", "Study spot added.", "OK");
            await Shell.Current.GoToAsync("//SpotsPage");
        }
        else
        {
            await DisplayAlertAsync("Sent", "Your spot was sent to the admin. It will appear after approval.", "OK");
            await LoadSubmissions();
        }
    }
}
