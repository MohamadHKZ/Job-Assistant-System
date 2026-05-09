using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("SystemSettings")]
public class SystemSetting
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = "";

    [Column(TypeName = "text")]
    public string Value { get; set; } = "";

    public DateTime UpdatedAt { get; set; }
}
