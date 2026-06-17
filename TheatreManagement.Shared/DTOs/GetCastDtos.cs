using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheatreManagement.Shared.DTOs.Employees;

namespace TheatreManagement.Shared.DTOs
{
    // для сгруппированного ответа
    public class CastWithRolesDto
    {
        public int CastId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public List<RoleGroupDto> Roles { get; set; }
        public string UserFullName { get; set; } = "";
        public int PlayId { get; set; }
    }

    public class RoleGroupDto
    {
        public int RoleInPlayId { get; set; }
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public List<EmployeeDto> Employees { get; set; }
    }
}
