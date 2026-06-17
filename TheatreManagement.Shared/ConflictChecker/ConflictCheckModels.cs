using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.ConflictChecker
{
    public class ConflictCheckRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Type { get; set; }

        public string? HallTypeName { get; set; }
        public int HallTypeId { get; set; }

        public List<int> EmployeeIds { get; set; } = new();
        public int? ExcludeEventId { get; set; }
    }

    public class ConflictCheckResponse
    {
        public bool HasConflicts { get; set; }
        public List<Warning> Warnings { get; set; } = new();
    }

    public class Warning
    {
        public ConflictType Type { get; set; }
        public string Message { get; set; }
        public int EmployeeId { get; set; }
    }

    public enum ConflictType
    {
        HallConflict,
        EmployeeConflict,
        EmployeeMissing,
        EmployeeIsntActive
    }
}
