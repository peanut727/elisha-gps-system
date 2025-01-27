using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    public int UserId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your first name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your last name")]
    public string LastName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your username")]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your password hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your email address")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    public string Role { get; set; } = "Customer";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsArchived { get; set; } = false;

    public DateTime? ArchivedAt { get; set; }
}
