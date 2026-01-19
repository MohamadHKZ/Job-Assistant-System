using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    public class ProfileDTO
    {
        public long ProfileId { get; set; }
        public List<string> SeekedJobTitle { get; set; } = new List<string>();
        public List<string> TechnicalSkills { get; set; } = new List<string>();
        public List<string> FieldSkills { get; set; } = new List<string>();
        public List<string> SoftSkills { get; set; } = new List<string>();
        public List<string> JobPositionSkills { get; set; } = new List<string>();
        public string Experience { get; set; } = string.Empty;
        public bool ReceiveNotifications { get; set; } = false;
    }
}
