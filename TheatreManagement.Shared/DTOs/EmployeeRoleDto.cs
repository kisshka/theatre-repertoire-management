using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class EmployeeRoleCreateDto
    {
        public int EmployeeId { get; set; }
        public int RoleInPlayId { get; set; }
    }
}
