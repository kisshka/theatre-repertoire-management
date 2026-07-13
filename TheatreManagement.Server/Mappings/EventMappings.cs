using System.Linq.Expressions;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.DTOs.Employees;
using TheatreManagement.Shared.ConflictChecker;
using TheatreManagement.Shared.DTOs;

namespace TheatreManagement.Server.Mappings
{
    public static class EventMappings
    {
        // Для GetEventsByDate и GetEventsByDateRange
        public static Expression<Func<Event, EventGetModel>> ToEventGetModelBasic = e => new EventGetModel
        {
            EventId = e.EventId,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Type = e.Type,
            LastEditTime = e.LastEditTime,
            CancellationReason = e.CancellationReason,
            DeletionTime = e.DeletionTime,
            UserFullName = e.User != null ? e.User.Surname + " " + e.User.Name + " " + e.User.FatherName : "",
            Plays = e.PlayEvents.Select(p => new PlayDto
            {
                Name = p.Play.Name
            }).ToList(),
            Stationar = e.Stationar != null ? new StationarDto
            {
                StationarId = e.Stationar.StationarId,
                Type = e.Stationar.Type,
                HallTypeId = e.Stationar.HallType.HallTypeId,
                HallTypeName = e.Stationar.HallType.Name
            } : null,
            Tour = e.Tour != null ? new TourDto
            {
                TourId = e.Tour.TourId,
                Country = e.Tour.Country,
                Area = e.Tour.Area
            } : null,
            Institution = e.Institution != null ? new InstitutionDto
            {
                InstitutionId = e.Institution.InstitutionId,
                Name = e.Institution.Name,
                Town = e.Institution.Town,
                Street = e.Institution.Street,
                House = e.Institution.House
            } : null,
            EmployeeRoles = e.EmployeeRoles.Select(er => er.EmployeeId).ToList()
        };

        // Для GetEventDetails
        public static Expression<Func<Event, EventGetModel>> ToEventGetModelDetails = e => new EventGetModel
        {
            EventId = e.EventId,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Type = e.Type,
            LastEditTime = e.LastEditTime,
            CancellationReason = e.CancellationReason,
            UserFullName = e.User != null ? e.User.Surname + " " + e.User.Name + " " + e.User.FatherName : "",
            Stationar = e.Stationar != null ? new StationarDto
            {
                StationarId = e.Stationar.StationarId,
                Type = e.Stationar.Type,
                HallTypeId = e.Stationar.HallTypeId,
                HallTypeName = e.Stationar.HallType != null ? e.Stationar.HallType.Name : ""
            } : null,
            Tour = e.Tour != null ? new TourDto
            {
                TourId = e.Tour.TourId,
                Country = e.Tour.Country,
                Area = e.Tour.Area
            } : null,
            Institution = e.Institution != null ? new InstitutionDto
            {
                InstitutionId = e.Institution.InstitutionId,
                Name = e.Institution.Name,
                Town = e.Institution.Town,
                Street = e.Institution.Street,
                House = e.Institution.House
            } : null,
            Plays = e.PlayEvents.Select(pe => new PlayDto
            {
                PlayId = pe.Play.PlayId,
                Name = pe.Play.Name,
                Duration = pe.Play.Duration,
                AgeCategory = pe.Play.AgeCategory
            }).ToList()
        };

        // Для GetEventForEdit
        public static Expression<Func<Event, EventPostModel>> ToEventPostModel = e => new EventPostModel
        {
            EventId = e.EventId,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Type = e.Type,
            CancellationReason = e.CancellationReason,
            Stationar = e.Stationar != null ? new StationarDto
            {
                StationarId = e.Stationar.StationarId,
                Type = e.Stationar.Type,
                HallTypeId = e.Stationar.HallTypeId,
                HallTypeName = e.Stationar.HallType != null ? e.Stationar.HallType.Name : ""
            } : null,
            Tour = e.Tour != null ? new TourDto
            {
                TourId = e.Tour.TourId,
                Country = e.Tour.Country,
                Area = e.Tour.Area
            } : null,
            Institution = e.Institution != null ? new InstitutionDto
            {
                InstitutionId = e.Institution.InstitutionId,
                Name = e.Institution.Name,
                Town = e.Institution.Town,
                Street = e.Institution.Street,
                House = e.Institution.House
            } : null,
            PlayEvents = e.PlayEvents.Select(pe => new PlayEventDto
            {
                PlayEventId = pe.PlayEventId,
                PlayId = pe.PlayId,
                EventId = pe.EventId,
                StartTime = pe.StartTime,
                EndTime = pe.EndTime,
                SelectedEmployees = e.EmployeeRoles
                    .Where(er => er.RoleInPlay.PlayId == pe.PlayId)
                    .Select(er => new EmployeeRoleSelectDto
                    {
                        EmployeeId = er.EmployeeId,
                        RoleInPlayId = er.RoleInPlayId,
                        RoleName = er.RoleInPlay.Name,
                        EmployeeFullName = er.Employee.Surname + " " + er.Employee.Name
                    }).ToList()
            }).ToList()
        };

        // Для CheckConflicts
        public static Expression<Func<Event, EventConflictPreview>> ToEventConflictPreview = e => new EventConflictPreview
        {
            EventId = e.EventId,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Type = e.Type,
            HallTypeId = e.Stationar != null && e.Stationar.HallType != null ? e.Stationar.HallType.HallTypeId : 0,
            EmployeeIds = e.EmployeeRoles.Select(er => er.EmployeeId).ToList()
        };

        //public static Expression<Func<PlayEvent, PlayWithRolesDto>> ToPlayWithRolesDto = pe => new PlayWithRolesDto
        //{
        //    PlayId = pe.Play.PlayId,
        //    PlayName = pe.Play.Name,
        //    RoleGroups = pe.Play.RoleInPlays.Select(role => new RoleGroupDto
        //    {
        //        RoleInPlayId = role.RoleInPlayId,
        //        RoleName = role.Name,
        //        RoleType = role.Type,
        //        Employees = pe.Event.EmployeeRoles
        //            .Where(er => er.RoleInPlayId == role.RoleInPlayId)
        //            .Select(er => new EmployeeDto
        //            {
        //                EmployeeId = er.EmployeeId,
        //                Surname = er.Employee.Surname,
        //                Name = er.Employee.Name,
        //                FatherName = er.Employee.FatherName,
        //                Post = er.Employee.Post
        //            }).ToList()
        //    }).ToList()
        //};
    }
}