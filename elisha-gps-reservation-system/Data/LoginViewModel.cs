using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter User Name")]
    public string? Username { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter password")]
    public string? Password { get; set; } = string.Empty;
}   