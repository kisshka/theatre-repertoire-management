using System.ComponentModel.DataAnnotations;


namespace TheatreManagement.Shared.DTOs
{
    public class UserModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? FatherName { get; set; }
    }

    public class RegisterModel : UserModel
    {
        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }


}