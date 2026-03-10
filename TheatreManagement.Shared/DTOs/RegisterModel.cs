using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace TheatreManagement.Shared.DTOs
{
    public class UserViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [DisplayName("Фамилия")]
        public string Surname { get; set; } = string.Empty;
        [Required]
        [DisplayName("Имя")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Отчество")]
        public string? FatherName { get; set; }
    }

    public class RegisterModel : UserViewModel
    {
        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }


}