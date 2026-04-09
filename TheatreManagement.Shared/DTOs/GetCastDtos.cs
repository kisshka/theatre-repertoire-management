using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    // для сгруппированного ответа
    public class CastWithRolesDto
    {
        public int CastId { get; set; }
        public string Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public List<RoleGroupDto> Roles { get; set; }
        public string UserFullName { get; set; } = "";
    }

    public class RoleGroupDto
    {
        public int RoleInPlayId { get; set; }
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public List<EmployeeDto> Employees { get; set; }
    }
}
