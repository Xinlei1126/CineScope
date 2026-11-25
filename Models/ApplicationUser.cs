using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CineScope.Models;
public class ApplicationUser : IdentityUser
{
    [StringLength(60)] public string? DisplayName { get; set; }
}
