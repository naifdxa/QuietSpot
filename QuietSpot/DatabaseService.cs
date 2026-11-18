using System.Security.Cryptography;
using System.Text;
using SQLite;

namespace QuietSpot;

// All database work (SQLite) is in this one class
public class DatabaseService
{
    public static DatabaseService Instance = new DatabaseService();

    // All the spots are in the same building (the library)
    const string LibraryMap = "https://maps.app.goo.gl/eCho8ZzeK8tTWhxk8?g_st=ic";

    SQLiteAsyncConnection db;

    // Turn the password into a long code (hash), so the real password is never saved
    static string HashPassword(string password, string username)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(password + "|" + username.ToLower());
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    async Task Init()
    {
        if (db != null)
            return;

        string path = Path.Combine(FileSystem.AppDataDirectory, "quietspot_v7.db3");
        db = new SQLiteAsyncConnection(path);
        await db.CreateTableAsync<StudySpot>();
        await db.CreateTableAsync<Review>();
        await db.CreateTableAsync<User>();
        await db.CreateTableAsync<SavedSpot>();

        // The admin account (username: admin, password: admin123)
        if (await db.Table<User>().CountAsync() == 0)
        {
            await db.InsertAsync(new User
            {
                FullName = "Admin",
                Username = "admin",
                PasswordHash = HashPassword("admin123", "admin"),
                Role = "Admin"
            });
        }

        // Starting data (UNITEN library), already approved
        if (await db.Table<StudySpot>().CountAsync() == 0)
        {
            Preferences.Default.Remove("last_viewed");

            await db.InsertAsync(new StudySpot
            {
                Name = "Library - Ground Floor",
                Location = "UNITEN Library, Ground Floor",
                MapLink = LibraryMap,
                OpeningHours = "Open 24 hours",
                NoiseLevel = "Moderate",
                Availability = "Available",
                LastUpdated = "Not updated yet",
                Description = "Always open, good for late night study and group work.",
                Status = "Approved",
                CreatedBy = "admin"
            });
            await db.InsertAsync(new StudySpot
            {
                Name = "Library - Level 2",
                Location = "UNITEN Library, Level 2",
                MapLink = LibraryMap,
                OpeningHours = "9 AM - 9 PM (Mon - Fri)",
                NoiseLevel = "Moderate",
                Availability = "Busy",
                LastUpdated = "Not updated yet",
                Description = "Study area on level 2.",
                Status = "Approved",
                CreatedBy = "admin"
            });
            await db.InsertAsync(new StudySpot
            {
                Name = "Library - Level 3",
                Location = "UNITEN Library, Level 3",
                MapLink = LibraryMap,
                OpeningHours = "9 AM - 9 PM (Mon - Fri)",
                NoiseLevel = "Quiet",
                Availability = "Available",
                LastUpdated = "Not updated yet",
                Description = "Quiet study area on level 3.",
                Status = "Approved",
                CreatedBy = "admin"
            });
            await db.InsertAsync(new StudySpot
            {
                Name = "Library - Level 4",
                Location = "UNITEN Library, Level 4",
                MapLink = LibraryMap,
                OpeningHours = "9 AM - 9 PM (Mon - Fri)",
                NoiseLevel = "Quiet",
                Availability = "Available",
                LastUpdated = "Not updated yet",
                Description = "Quiet study area on level 4.",
                Status = "Approved",
                CreatedBy = "admin"
            });
            await db.InsertAsync(new StudySpot
            {
                Name = "Library - Level 5",
                Location = "UNITEN Library, Level 5",
                MapLink = LibraryMap,
                OpeningHours = "9 AM - 9 PM (Mon - Fri)",
                NoiseLevel = "Quiet",
                Availability = "Available",
                LastUpdated = "Not updated yet",
                Description = "Quiet study area on the top level.",
                Status = "Approved",
                CreatedBy = "admin"
            });
        }
    }

    // ---------- Users ----------

    // Returns an error message, or "" if the account was created
    public async Task<string> RegisterAsync(string fullName, string username, string password)
    {
        await Init();

        string uname = username.Trim().ToLower();

        var old = await db.Table<User>().Where(u => u.Username == uname).FirstOrDefaultAsync();
        if (old != null)
            return "This username is already taken.";

        User user = new User();
        user.FullName = fullName.Trim();
        user.Username = uname;
        user.PasswordHash = HashPassword(password, uname);
        user.Role = "Student";

        await db.InsertAsync(user);
        return "";
    }

    // Returns the user if the username and password are right, otherwise null
    public async Task<User> LoginAsync(string username, string password)
    {
        await Init();

        string uname = username.Trim().ToLower();
        string hash = HashPassword(password, uname);

        return await db.Table<User>()
            .Where(u => u.Username == uname && u.PasswordHash == hash)
            .FirstOrDefaultAsync();
    }

    // ---------- Manage users (admin only) ----------

    // All the users, oldest first
    public async Task<List<User>> GetUsersAsync()
    {
        await Init();
        return await db.Table<User>().OrderBy(u => u.Id).ToListAsync();
    }

    // Give a user a new role: "Student" or "Admin"
    public async Task SetUserRoleAsync(int userId, string role)
    {
        await Init();
        var user = await db.Table<User>().Where(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return;

        user.Role = role;
        await db.UpdateAsync(user);
    }

    // Delete the user and his saved spots (his reviews stay)
    public async Task DeleteUserAsync(User user)
    {
        await Init();
        await db.Table<SavedSpot>().DeleteAsync(x => x.UserId == user.Id);
        await db.DeleteAsync(user);
    }

    // ---------- Study spots ----------

    // Only the approved spots (what the students see)
    public async Task<List<StudySpot>> GetSpotsAsync()
    {
        await Init();
        return await db.Table<StudySpot>().Where(s => s.Status == "Approved").ToListAsync();
    }

    // Spots waiting for the admin
    public async Task<List<StudySpot>> GetPendingSpotsAsync()
    {
        await Init();
        return await db.Table<StudySpot>().Where(s => s.Status == "Pending").ToListAsync();
    }

    // The spots added by one user (any status)
    public async Task<List<StudySpot>> GetMySpotsAsync(string username)
    {
        await Init();
        return await db.Table<StudySpot>().Where(s => s.CreatedBy == username).ToListAsync();
    }

    public async Task<StudySpot> GetSpotAsync(int id)
    {
        await Init();
        return await db.Table<StudySpot>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }

    // Id = 0 means new spot (insert), otherwise update the old one
    public async Task SaveSpotAsync(StudySpot spot)
    {
        await Init();
        if (spot.Id == 0)
            await db.InsertAsync(spot);
        else
            await db.UpdateAsync(spot);
    }

    // Delete the spot, its reviews and the saved rows
    public async Task DeleteSpotAsync(StudySpot spot)
    {
        await Init();
        await db.Table<Review>().DeleteAsync(r => r.SpotId == spot.Id);
        await db.Table<SavedSpot>().DeleteAsync(x => x.SpotId == spot.Id);
        await db.DeleteAsync(spot);
    }

    // ---------- Saved spots (each user has his own list) ----------

    public async Task<bool> IsSavedAsync(int userId, int spotId)
    {
        await Init();
        int count = await db.Table<SavedSpot>()
            .Where(x => x.UserId == userId && x.SpotId == spotId)
            .CountAsync();
        return count > 0;
    }

    // Save the spot if it is not saved, remove it if it is saved
    public async Task ToggleSavedAsync(int userId, int spotId)
    {
        await Init();
        var old = await db.Table<SavedSpot>()
            .Where(x => x.UserId == userId && x.SpotId == spotId)
            .FirstOrDefaultAsync();

        if (old == null)
            await db.InsertAsync(new SavedSpot { UserId = userId, SpotId = spotId });
        else
            await db.DeleteAsync(old);
    }

    public async Task<List<StudySpot>> GetSavedSpotsAsync(int userId)
    {
        await Init();

        var saved = await db.Table<SavedSpot>().Where(x => x.UserId == userId).ToListAsync();
        var result = new List<StudySpot>();

        foreach (var item in saved)
        {
            var spot = await GetSpotAsync(item.SpotId);
            if (spot != null && spot.Status == "Approved")
            {
                spot.IsSaved = true;
                result.Add(spot);
            }
        }

        return result;
    }

    // ---------- Reviews ----------

    // All reviews of one spot, newest first
    public async Task<List<Review>> GetReviewsAsync(int spotId)
    {
        await Init();
        return await db.Table<Review>()
            .Where(r => r.SpotId == spotId)
            .OrderByDescending(r => r.Id)
            .ToListAsync();
    }

    // One review per user per spot: a new review, or update the old one.
    // Returns true if the old review was updated.
    public async Task<bool> AddOrUpdateReviewAsync(Review review)
    {
        await Init();

        var old = await db.Table<Review>()
            .Where(r => r.SpotId == review.SpotId && r.UserId == review.UserId)
            .FirstOrDefaultAsync();

        if (old == null)
        {
            await db.InsertAsync(review);
        }
        else
        {
            old.Name = review.Name;
            old.Rating = review.Rating;
            old.Comment = review.Comment;
            old.CreatedAt = review.CreatedAt;
            await db.UpdateAsync(old);
        }

        // Calculate the average rating of the spot again
        var reviews = await db.Table<Review>().Where(r => r.SpotId == review.SpotId).ToListAsync();
        var spot = await GetSpotAsync(review.SpotId);

        spot.ReviewCount = reviews.Count;
        spot.AverageRating = reviews.Average(r => r.Rating);
        await db.UpdateAsync(spot);

        return old != null;
    }
}

