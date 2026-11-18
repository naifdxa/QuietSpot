using SQLite;

namespace QuietSpot;

// One row = one user account
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";   // we never save the real password
    public string Role { get; set; } = "Student";    // Student or Admin

    // Only used for showing things on screen (not stored)
    [Ignore]
    public string RoleText => "Role: " + Role;

    [Ignore]
    public string RoleButtonText => Role == "Admin" ? "Make Student" : "Make Admin";
}
