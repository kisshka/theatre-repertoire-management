using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Employees
{
    public class EmployeeRoleCreateDto
    {
        public int EmployeeId { get; set; }
        public int RoleInPlayId { get; set; }

        public int? CastId { get; set; }
        public int? PlayEventId { get; set; }
    }

}
