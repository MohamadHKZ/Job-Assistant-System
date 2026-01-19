using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    public class EmbeddedProfile
    {
        [Key]
        public int ProfileId { get; set; }
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedTechnicalSkills { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedFieldSkills { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedJobPositionSkills { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedJobTitle { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedSoftSkills { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]

        public Profile Profile { get; set; } = null!;
    }
}
