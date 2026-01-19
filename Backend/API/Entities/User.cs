using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public byte[] PasswordHash { get; set; } = new byte[0];
    public byte[] PasswordSalt { get; set; } = new byte[0];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}