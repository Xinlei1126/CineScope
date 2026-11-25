using System.ComponentModel.DataAnnotations;
namespace CineScope.ViewModels;
public class RegisterViewModel
{
    [Required, StringLength(60)] public string DisplayName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(8)] public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}
public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class MovieReviewInputViewModel
{
    [Range(1, 10)] public int Rating { get; set; }
    [Required, StringLength(1200, MinimumLength = 3)] public string Body { get; set; } = "";
}
