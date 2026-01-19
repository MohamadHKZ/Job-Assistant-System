using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities
{
    public class ProfileQualifications
    {
        [Key]
        public int ProfileId { get; set; }
        [Column(TypeName = "jsonb")]
        public List<string> SeekedJobTitle { get; set; } = new List<string>();
        [Column(TypeName = "jsonb")]
        public List<string> TechnicalSkills { get; set; } = new List<string>();
        [Column(TypeName = "jsonb")]
        public List<string> FieldSkills { get; set; } = new List<string>();
        [Column(TypeName = "jsonb")]
        public List<string> SoftSkills { get; set; } = new List<string>();
        [Column(TypeName = "jsonb")]
        public List<string> JobPositionSkills { get; set; } = new List<string>();
        public string Experience { get; set; } = string.Empty;
        public Profile Profile { get; set; } = null!;
    }
}
