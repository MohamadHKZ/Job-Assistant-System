using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }
        public bool ReceiveNotifications { get; set; }
        public bool IsActive { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ProfileQualifications ProfileQualifications { get; set; } = null!;
    }
}
