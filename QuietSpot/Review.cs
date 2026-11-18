using SQLite;

namespace QuietSpot;

// One row = one review written by one user
public class Review
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int SpotId { get; set; }      // which study spot this review is for
    public int UserId { get; set; }      // which user wrote it
    public string Name { get; set; } = "";
    public int Rating { get; set; }      // 1 to 5
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    // Example: 4 filled stars and 1 empty star
    [Ignore]
    public string Stars => new string('\u2605', Rating) + new string('\u2606', 5 - Rating);

    // Example: 2 days ago
    [Ignore]
    public string TimeAgo
    {
        get
        {
            TimeSpan diff = DateTime.Now - CreatedAt;

            if (diff.TotalMinutes < 1)
                return "just now";
            if (diff.TotalHours < 1)
                return Ago((int)diff.TotalMinutes, "minute");
            if (diff.TotalDays < 1)
                return Ago((int)diff.TotalHours, "hour");
            if (diff.TotalDays < 30)
                return Ago((int)diff.TotalDays, "day");
            return CreatedAt.ToString("dd MMM yyyy");
        }
    }

    static string Ago(int number, string unit)
    {
        if (number == 1)
            return "1 " + unit + " ago";
        return number + " " + unit + "s ago";
    }
}
