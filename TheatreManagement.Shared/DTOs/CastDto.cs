using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreManagement.Shared.DTOs.Employees;

namespace TheatreManagement.Shared.DTOs
{
    public class CastDto
    {
        public int CastId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string? Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }

        // ссылки
        //public string? UserId { get; set; }
        //public UserDto? User { get; set; }
        public int PlayId { get; set; }
        public PlayDto? Play { get; set; }

        public List<EmployeeRoleCreateDto> EmployeeRolesCreate { get; set; } = new();
        //public List<EmployeeRoleDto> EmployeeRolesDto { get; set; } = new();
    }

}
