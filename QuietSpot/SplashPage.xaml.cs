namespace QuietSpot;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    // Wait 3 seconds then go to the Login page automatically
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(3000);
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
