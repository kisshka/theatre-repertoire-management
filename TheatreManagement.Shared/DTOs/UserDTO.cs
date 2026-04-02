using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace TheatreManagement.Shared.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [DisplayName("Фамилия")]
        public string Surname { get; set; } = string.Empty;
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [DisplayName("Имя")]
        public string Name { get; set; } = string.Empty;
        [DisplayName("Отчество")]
        public string? FatherName { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [DisplayName("Роль")]
        public string Role { get; set; } = string.Empty;
    }

    public class RegisterModel : UserDto
    {
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }


}