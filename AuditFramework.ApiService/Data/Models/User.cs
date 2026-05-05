using System.ComponentModel.DataAnnotations;

namespace AuditFramework.ApiService.Data.Models;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    public string? Bio { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "user";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
