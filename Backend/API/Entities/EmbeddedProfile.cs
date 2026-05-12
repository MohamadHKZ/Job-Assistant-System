using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

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
        [Column(TypeName = "vector(1024)")]
        public Vector EmbeddedJobTitle { get; set; } = new(new float[1024]);
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedSoftSkills { get; set; } = new List<SkillEmbedding>();
        [Column(TypeName = "jsonb")]
        public List<SkillEmbedding> EmbeddedTechnologies { get; set; } = new List<SkillEmbedding>();
        public Profile Profile { get; set; } = null!;
    }
}
