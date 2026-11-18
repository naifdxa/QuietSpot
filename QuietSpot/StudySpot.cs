using SQLite;

namespace QuietSpot;

// One row in the database = one study spot
public class StudySpot
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public string MapLink { get; set; } = "";
    public string OpeningHours { get; set; } = "";
    public string NoiseLevel { get; set; } = "";
    public string Availability { get; set; } = "";
    public string LastUpdated { get; set; } = "";
    public string Description { get; set; } = "";
    public string PhotoPath { get; set; } = "";
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Pending = waiting for the admin, Approved = everyone can see it, Rejected = not accepted
    public string Status { get; set; } = "Pending";
    public string CreatedBy { get; set; } = "";     // username of the student who added it

    // The next properties are only used for showing things on screen (not stored)

    // True if the logged in user saved this spot (filled from the SavedSpot table)
    [Ignore]
    public bool IsSaved { get; set; }

    // Example: 4.3
    [Ignore]
    public string AverageText => ReviewCount == 0 ? "-" : AverageRating.ToString("0.0");

    // Example: 4 filled stars and 1 empty star
    [Ignore]
    public string StarsText
    {
        get
        {
            int full = (int)Math.Round(AverageRating);
            return new string('\u2605', full) + new string('\u2606', 5 - full);
        }
    }

    // Example: Based on 25 reviews
    [Ignore]
    public string ReviewsCountText
    {
        get
        {
            if (ReviewCount == 0)
                return "No reviews yet";
            if (ReviewCount == 1)
                return "Based on 1 review";
            return "Based on " + ReviewCount + " reviews";
        }
    }

    // Short text used in the cards
    [Ignore]
    public string RatingText
    {
        get
        {
            if (ReviewCount == 0)
                return "No ratings yet";
            return StarsText + "  " + AverageText + " (" + ReviewCount + ")";
        }
    }

    [Ignore]
    public string SaveButtonText => IsSaved ? "Remove from Saved" : "Save this Spot";

    // Used in the "My submissions" list
    [Ignore]
    public string SubmissionText => Name + "  -  " + Status;

    // Which picture to show for this spot
    [Ignore]
    public string ImageName
    {
        get
        {
            // 1) a photo chosen by the user
            if (!string.IsNullOrEmpty(PhotoPath) && File.Exists(PhotoPath))
                return PhotoPath;

            // 2) one picture for each floor of the library
            if (Name.Contains("Ground Floor"))
                return "library_ground.png";
            if (Name.Contains("Level 2"))
                return "library_level2.png";
            if (Name.Contains("Level 3"))
                return "library_level3.png";
            if (Name.Contains("Level 4"))
                return "library_level4.png";
            if (Name.Contains("Level 5"))
                return "library_level5.png";

            // 3) other spots: picture by noise level
            if (NoiseLevel == "Quiet")
                return "spot_quiet.png";
            if (NoiseLevel == "Moderate")
                return "spot_moderate.png";
            return "spot_noisy.png";
        }
    }

    // The Google Maps link (if no link was saved, we search the location text)
    [Ignore]
    public string MapUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(MapLink))
                return MapLink;
            return "https://www.google.com/maps/search/?api=1&query=" + Uri.EscapeDataString(Location);
        }
    }
}
