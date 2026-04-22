using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Users
{
    public class RegisterModel
    {
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

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
