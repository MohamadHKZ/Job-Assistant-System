using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("UserRoles")]
public class UserRole
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
