using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Survey.Infrastructure.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(IsActive))]
[Table("admins")]
public class Admin : BaseEntity
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string PasswordHash { get; set; }
    [Required]
    public required string FullName { get; set; }
    public bool IsActive { get; set; }
}