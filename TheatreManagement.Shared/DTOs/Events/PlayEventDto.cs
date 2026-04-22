using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class PlayEventDto 
    {
        public int PlayEventId { get; set; }

        public int PlayId { get; set; }
        public int EventId { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public List<EmployeeRoleSelectDto> SelectedEmployees { get; set; } = new();

        public List<PlayEventRoleDisplay> RoleDisplays => BuildRoleDisplays();

        private List<PlayEventRoleDisplay> BuildRoleDisplays()
        {
            return SelectedEmployees
                .GroupBy(er => er.RoleInPlayId)
                .Select(g => new PlayEventRoleDisplay
                {
                    RoleName = g.First().RoleName,
                    EmployeeNames = string.Join(", ", g.Select(er => er.EmployeeFullName)),
                    Count = g.Count()
                })
                .ToList();
        }

        public int CastId { get; set; }
    }

    public class PlayEventRoleDisplay
    {
        public string? RoleName { get; set; }
        public string? EmployeeNames { get; set; }
        public int Count { get; set; }
    }

    public class EmployeeRoleSelectDto
    {
        public int EmployeeId { get; set; }
        public int RoleInPlayId { get; set; }
        public string? EmployeeFullName { get; set; }
        public string? RoleName { get; set; }
    }

}
