namespace QuietSpot;

// Remembers who is logged in while the app is open
public static class Session
{
    public static User CurrentUser { get; set; }

    public static bool IsLoggedIn => CurrentUser != null;

    public static bool IsAdmin => CurrentUser != null && CurrentUser.Role == "Admin";
}
