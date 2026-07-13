
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs.Availability;

namespace TheatreManagement.Server.Services
{
    public class AvailabilityService
    {
        private readonly DataContext _context;

        public AvailabilityService(DataContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PlayAvailabilityDto>> GetPlaysAvailabilityAsync(
            DateTime startTime,
            DateTime endTime,
            int page = 1,
            int pageSize = 10,
            string? searchText = null)
        {
            var playsQuery = _context.Plays
                .Include(p => p.Casts)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizedSearch = searchText.Trim().ToLower();
                playsQuery = playsQuery.Where(p =>
                    DataContext.CustomLike(p.Name, normalizedSearch));
            }

            var totalCount = await playsQuery.CountAsync();

            var pagedPlays = await playsQuery
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new { p.PlayId, p.Name })
                .ToListAsync();

            if (!pagedPlays.Any())
            {
                return new PagedResult<PlayAvailabilityDto>
                {
                    Items = new List<PlayAvailabilityDto>(),
                    TotalCount = totalCount
                };
            }

            var playIds = pagedPlays.Select(p => p.PlayId).ToList();

            // Получаем всех занятых сотрудников за указанный период
            var busyEmployees = await _context.EmployeeRoles
                .Where(er => er.Event != null &&
                             er.Event.StartTime < endTime &&
                             er.Event.EndTime > startTime)
                .Select(er => new
                {
                    er.EmployeeId,
                    er.EventId,
                    EventName = er.Event.PlayEvents.FirstOrDefault().Play.Name,
                    EventStart = er.Event.StartTime,
                    EventEnd = er.Event.EndTime
                })
                .ToDictionaryAsync(k => k.EmployeeId, v => v);

            // Получаем все составы для выбранных спектаклей
            var casts = await _context.Castes
                .Where(c => playIds.Contains(c.PlayId) && c.DeletionTime == null)
                .Include(c => c.EmployeeRoles)
                    .ThenInclude(cr => cr.RoleInPlay)
                .Include(c => c.EmployeeRoles)
                    .ThenInclude(cr => cr.Employee)
                .ToListAsync();

            var result = new List<PlayAvailabilityDto>();

            foreach (var play in pagedPlays)
            {
                var playCasts = casts.Where(c => c.PlayId == play.PlayId).ToList();
                var castDtos = new List<CastAvailabilityDto>();

                foreach (var cast in playCasts)
                {
                    var roleDtos = new List<RoleAvailabilityDto>();
                    int availableCount = 0;

                    foreach (var castRole in cast.EmployeeRoles)
                    {
                        bool isAvailable = !busyEmployees.ContainsKey(castRole.EmployeeId);
                        string? conflict = null;

                        if (!isAvailable && busyEmployees.TryGetValue(castRole.EmployeeId, out var busyEvent))
                        {
                            conflict = $"Занят в {busyEvent.EventName} {busyEvent.EventStart:HH:mm}-{busyEvent.EventEnd:HH:mm}";
                        }

                        if (isAvailable) availableCount++;

                        roleDtos.Add(new RoleAvailabilityDto
                        {
                            RoleInPlayId = castRole.RoleInPlayId,
                            RoleName = castRole.RoleInPlay?.Name ?? "",
                            EmployeeId = castRole.EmployeeId,
                            EmployeeFullName = $"{castRole.Employee?.Surname} {castRole.Employee?.Name}",
                            IsAvailable = isAvailable,
                            ConflictDescription = conflict
                        });
                    }

                    var status = availableCount == roleDtos.Count ? CastStatus.FullAvailable :
                                (availableCount > 0 ? CastStatus.PartialAvailable : CastStatus.NotAvailable);

                    castDtos.Add(new CastAvailabilityDto
                    {
                        CastId = cast.CastId,
                        CastName = cast.Name,
                        Roles = roleDtos,
                        Status = status,
                        AvailableCount = availableCount,
                        TotalCount = roleDtos.Count
                    });
                }

                result.Add(new PlayAvailabilityDto
                {
                    PlayId = play.PlayId,
                    PlayName = play.Name,
                    Casts = castDtos,
                    HasAvailableCast = castDtos.Any(c => c.Status == CastStatus.FullAvailable)
                });
            }

            return new PagedResult<PlayAvailabilityDto>
            {
                Items = result,
                TotalCount = totalCount
            };
        }
    }
}