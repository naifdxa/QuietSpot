using SQLite;

namespace QuietSpot;

// One row = one user saved one spot
public class SavedSpot
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int SpotId { get; set; }
}
