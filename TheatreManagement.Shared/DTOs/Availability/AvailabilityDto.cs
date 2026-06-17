
namespace TheatreManagement.Shared.DTOs.Availability
{
    public class CastAvailabilityDto
    {
        public int CastId { get; set; }
        public string CastName { get; set; } = string.Empty;
        public List<RoleAvailabilityDto> Roles { get; set; } = new();
        public CastStatus Status { get; set; }
        public int AvailableCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class RoleAvailabilityDto
    {
        public int RoleInPlayId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeFullName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? ConflictDescription { get; set; }
    }

    public class PlayAvailabilityDto
    {
        public int PlayId { get; set; }
        public string PlayName { get; set; } = string.Empty;
        public List<CastAvailabilityDto> Casts { get; set; } = new();
        public PlayStatus PlayStatus { get; set; }
    }

    public enum CastStatus
    {
        FullAvailable,
        PartialAvailable,
        NotAvailable 
    }

    public enum PlayStatus
    {
        HasAvailableCast,
        NoAvailableCast
    }
}