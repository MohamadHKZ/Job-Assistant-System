using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class JobSource
{
    [Key]
    public string SourceName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
