using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    // DTO для сгруппированного ответа
    public class CastWithRolesDto
    {
        public int CastId { get; set; }
        public string Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public List<RoleGroupDto> Roles { get; set; }
    }

    public class RoleGroupDto
    {
        public int RoleInPlayId { get; set; }
        public string RoleName { get; set; }
        public string RoleType { get; set; }
        public List<EmployeeDto> Employees { get; set; }
    }

    //public class EmployeeRoleDetailDto
    //{
    //    //public int EmployeeRoleId { get; set; }
    //    public int EmployeeId { get; set; }
    //    public string EmployeeSurname { get; set; }
    //    public string EmployeeName { get; set; }
    //    public string EmployeeFatherName { get; set; }
    //    public string EmployeePost { get; set; }
    //}
}
