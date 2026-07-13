using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.ConflictChecker;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Server.Mappings;
using TheatreManagement.Shared.DTOs.Employees;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly DataContext _context;

        public EventsController(UserManager<User> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("hall-types")]
        public async Task<ActionResult<List<HallTypeDto>>> GetHallTypes()
        {
            var hallTypes = await _context.HallTypes
                .OrderBy(h => h.Name)
                .Select(h => new HallTypeDto
                {
                    Id = h.HallTypeId,
                    Name = h.Name
                })
                .ToListAsync();

            return Ok(hallTypes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventPostModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            // Валидация
            var validPlayEvents = model.PlayEvents.Where(pe => pe.PlayId > 0).ToList();

            if (!validPlayEvents.Any())
            {
                return BadRequest("Добавьте хотя бы один спектакль");
            }

            foreach (var playEvent in validPlayEvents)
            {
                if (playEvent.SelectedEmployees == null || !playEvent.SelectedEmployees.Any())
                {
                    return BadRequest($"Для спектакля выберите состав");
                }
            }

            // Добавление
            try
            {
                var newEvent = new Event
                {
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    LastEditTime = DateTime.Now,
                    CancellationReason = null,
                    User = currentUser,
                    DeletionTime = null,
                    Type = model.Type
                };

                // Специфичные данные
                switch (model.Type)
                {
                    case "stationar":
                        newEvent.Stationar = new Stationar
                        {
                            HallType = await _context.HallTypes
                                .FirstOrDefaultAsync(ht => ht.HallTypeId == model.Stationar.HallTypeId),
                            Type = model.Stationar.Type
                        };
                        break;

                    case "tour":
                        newEvent.Tour = new Tour
                        {
                            Country = model.Tour.Country,
                            Area = model.Tour.Area
                        };
                        break;

                    case "visit":
                        // Выбирается существующее или создается новое
                        if (model.Institution.InstitutionId > 0)
                        {
                            var existingInstitution = await _context.Institutions
                                .FindAsync(model.Institution.InstitutionId);

                            newEvent.Institution = existingInstitution;
                        }
                        else
                        {
                            newEvent.Institution = new Institution
                            {
                                Name = model.Institution.Name,
                                Town = model.Institution.Town ?? "",
                                Street = model.Institution.Street ?? "",
                                House = model.Institution.House ?? ""
                            };
                        }
                        break;
                }

                // Связи со спектклями
                foreach (var playEventDto in model.PlayEvents)
                {
                    if (playEventDto.PlayId <= 0) continue;

                    newEvent.PlayEvents.Add(new PlayEvent
                    {
                        PlayId = playEventDto.PlayId,
                        StartTime = playEventDto.StartTime,
                        EndTime = playEventDto.EndTime
                    });
                }

                // Назначение сотрудников на роли
                foreach (var playEventDto in model.PlayEvents)
                {
                    foreach (var selectedEmployee in playEventDto.SelectedEmployees)
                    {
                        if (selectedEmployee.EmployeeId <= 0) continue;

                        var employee = await _context.Employees.FindAsync(selectedEmployee.EmployeeId);
                        if (employee == null)
                            return BadRequest($"Сотрудник с ID {selectedEmployee.EmployeeId} не найден");

                        var roleInPlay = await _context.RoleInPlays.FindAsync(selectedEmployee.RoleInPlayId);
                        if (roleInPlay == null)
                            return BadRequest($"Роль с ID {selectedEmployee.RoleInPlayId} не найдена");

                        var employeeRole = new EmployeeRole
                        {
                            EmployeeId = selectedEmployee.EmployeeId,
                            RoleInPlayId = selectedEmployee.RoleInPlayId,
                            Event = newEvent,
                            CastId = null
                        };

                        _context.EmployeeRoles.Add(employeeRole);
                    }
                }

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            { 
            return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<List<EventGetModel>>> GetEventsByDate(DateTime date)
        {
            var startTime = date.Date;
            var nextDay = startTime.AddDays(1);

            var events = await _context.Events
                .Where(e => e.StartTime < nextDay && e.EndTime > startTime)
                .OrderBy(e => e.StartTime)
                .Include(e => e.PlayEvents)
                .Include(e => e.EmployeeRoles)
                .Select(EventMappings.ToEventGetModelBasic)
                .ToListAsync();

            // БЫстрая проверка на наличие хотя бы одного конфликта
            SimpleCheckConflicts(events);

            return Ok(events);
        }

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<EventGetModel>>> GetAllEvents([FromQuery] string? searchText = null,
                                                                                 [FromQuery] int page = 1,
                                                                                 [FromQuery] int pageSize = 10,
                                                                                 [FromQuery] bool isArchive = false)
        {
            var query = _context.Events
                .Include(e => e.User)
                .Include(e => e.PlayEvents)
                .Include(e => e.EmployeeRoles)
                .Include(e => e.Stationar)
                    .ThenInclude(s => s.HallType)
                .Include(e => e.Tour)
                .Include(e => e.Institution)
                .AsQueryable();

            if (isArchive)
            {
                query = query.IgnoreQueryFilters().Where(e => e.DeletionTime != null);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var normalizedSearch = searchText.Trim().ToLower();
                query = query.Where(e =>
                    DataContext.CustomLike(e.Type, normalizedSearch) ||
                    e.PlayEvents.Any(pe => DataContext.CustomLike(pe.Play.Name, normalizedSearch)) ||
                    (e.Stationar != null && DataContext.CustomLike(e.Stationar.Type, normalizedSearch)) ||
                    (e.Tour != null && (DataContext.CustomLike(e.Tour.Country, normalizedSearch) || DataContext.CustomLike(e.Tour.Area, normalizedSearch))) ||
                    (e.Institution != null && DataContext.CustomLike(e.Institution.Name, normalizedSearch)));
            }

            var totalCount = await query.CountAsync();

            var events = await query
                .OrderByDescending(e => e.LastEditTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(EventMappings.ToEventGetModelBasic)
                .ToListAsync();

            return Ok(new PagedResult<EventGetModel>
            {
                Items = events,
                TotalCount = totalCount
            });
        }

        [HttpGet("range")]
        public async Task<ActionResult<List<EventGetModel>>> GetEventsByDateRange([FromQuery] string start, [FromQuery] string end)
        {
            if (!DateTime.TryParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                return BadRequest("Неверный формат даты");
            }

            var events = await _context.Events
                .Where(e => e.StartTime < endDate.AddDays(1) && e.EndTime > startDate)
                .Include(e => e.PlayEvents)
                .Include(e => e.EmployeeRoles)
                .OrderBy(e => e.StartTime)
                .Select(EventMappings.ToEventGetModelBasic)
                .ToListAsync();

            SimpleCheckConflicts(events);

            return Ok(events);
        }

        private void SimpleCheckConflicts(List<EventGetModel> events)
        {
            // Конфликты залов
            var hallEvents = events.Where(e => e.Type == "stationar")
                                   .ToList();

            for (int i = 0; i < hallEvents.Count; i++)
            {
                if (hallEvents[i].HasConflict) continue;

                for (int j = i + 1; j < hallEvents.Count; j++)
                {
                    if (hallEvents[j].HasConflict) continue;

                    var a = hallEvents[i];
                    var b = hallEvents[j];

                    if (a.EventId != b.EventId &&
                        a.Stationar!.HallTypeId == b.Stationar!.HallTypeId &&
                        IsConflicting(a.StartTime!.Value, a.EndTime!.Value, b.StartTime!.Value, b.EndTime!.Value))
                    {
                        a.HasConflict = true;
                        b.HasConflict = true;
                        break;
                    }
                }
            }

            // Конфликты сотрудников
            var employeeEvents = events
                .SelectMany(e => e.EmployeeRoles
                    .Select(empId => new { EmployeeId = empId, Event = e }))
                .GroupBy(x => x.EmployeeId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Select(x => x.Event).ToList());

            foreach (var empEvents in employeeEvents)
            {
                for (int i = 0; i < empEvents.Count; i++)
                {
                    if (empEvents[i].HasConflict) continue;

                    for (int j = i + 1; j < empEvents.Count; j++)
                    {
                        if (empEvents[j].HasConflict) continue;

                        var a = empEvents[i];
                        var b = empEvents[j];

                        if (a.EventId != b.EventId &&
                        IsConflicting(a.StartTime!.Value, a.EndTime!.Value, b.StartTime!.Value, b.EndTime!.Value))
                        {
                            a.HasConflict = true;
                            b.HasConflict = true;
                            break;
                        }
                    }
                }
            }
        }

        [HttpPost("check-conflicts")]
        public async Task<ActionResult<ConflictCheckResponse>> CheckConflicts([FromBody] ConflictCheckRequest request)
        {
            var warnings = new List<Warning>();

            var existingEvents = await _context.Events
                .Where(e => e.StartTime < request.EndTime && e.EndTime > request.StartTime)
                .Select(EventMappings.ToEventConflictPreview)
                .ToListAsync();

            // Исключить текущее мероприятие при редактировании
            if (request.ExcludeEventId.HasValue)
            {
                existingEvents = existingEvents.Where(e => e.EventId != request.ExcludeEventId.Value).ToList();
            }

            // Проверка залов
            if (request.Type == "stationar" && !string.IsNullOrEmpty(request.HallTypeName))
            {
                var hallConflicts = existingEvents
                    .Where(e => e.Type == "stationar" && e.HallTypeId == request.HallTypeId &&
                                IsConflicting(request.StartTime, request.EndTime, e.StartTime, e.EndTime))
                    .Select(e => $"{request.HallTypeName} уже занят {e.StartTime:dd.MM HH:mm} - {e.EndTime:HH:mm}")
                    .ToList();

                foreach (var conflict in hallConflicts)
                {
                    warnings.Add(new Warning { Message = conflict, Type = ConflictType.HallConflict });
                }
            }

            // Проверка сотрудников
            if (request.EmployeeIds.Any())
            {

                var employees = await _context.Employees
                    .Where(e => request.EmployeeIds.Contains(e.EmployeeId))
                    .ToDictionaryAsync(e => e.EmployeeId, e => $"{e.Surname} {e.Name} {e.FatherName}");

                var employeeConflicts = existingEvents
                    .Where(e => e.EmployeeIds.Intersect(request.EmployeeIds).Any() &&
                                IsConflicting(request.StartTime, request.EndTime, e.StartTime, e.EndTime))
                    .SelectMany(e => e.EmployeeIds.Intersect(request.EmployeeIds).Select(empId => new { e, empId }))
                    .ToList();

                foreach (var conflict in employeeConflicts)
                {
                    var employeeName = employees.GetValueOrDefault(conflict.empId);
                    warnings.Add(new Warning
                    {
                        Message = $"{employeeName} уже занят {conflict.e.StartTime:dd.MM HH:mm} - {conflict.e.EndTime:HH:mm}",
                        Type = ConflictType.EmployeeConflict,
                        EmployeeId = conflict.empId
                    });
                }
            }

            return Ok(new ConflictCheckResponse
            {
                HasConflicts = warnings.Any(),
                Warnings = warnings
            });
        }

        private bool IsConflicting(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 < end2 && start2 < end1;
        }

        [HttpGet("{eventId}/details")]
        public async Task<ActionResult<EventGetModel>> GetEventDetails(int eventId)
        {
            var eventEntity = await _context.Events
                .Where(e => e.EventId == eventId)
                .Include(e => e.User)
                .Select(EventMappings.ToEventGetModelDetails)
                .FirstOrDefaultAsync();

            if (eventEntity == null)
            {
                return NotFound($"Мероприятие с ID {eventId} не найдено");
            }

            var playsWithRoles = await _context.PlayEvents
                          .Where(pe => pe.EventId == eventId)
                          .Select(pe => new PlayWithRolesDto
                          {
                              PlayId = pe.Play.PlayId,
                              PlayName = pe.Play.Name,
                              RoleGroups = pe.Play.RoleInPlays.Select(role => new RoleGroupDto
                              {
                                  RoleInPlayId = role.RoleInPlayId,
                                  RoleName = role.Name,
                                  RoleType = role.Type,
                                  Employees = _context.EmployeeRoles
                                      .Where(er => er.EventId == eventId && er.RoleInPlayId == role.RoleInPlayId)
                                      .Select(er => new EmployeeDto
                                      {
                                          EmployeeId = er.EmployeeId,
                                          Surname = er.Employee.Surname,
                                          Name = er.Employee.Name,
                                          FatherName = er.Employee.FatherName,
                                          Post = er.Employee.Post
                                      })
                                      .ToList()
                              }).ToList()
                          })
                          .ToListAsync();

            eventEntity.PlaysWithRoles = playsWithRoles;

            return Ok(eventEntity);
        }

        [HttpGet("{eventId}/for-edit")]
        public async Task<ActionResult<EventPostModel>> GetEventForEdit(int eventId)
        {
            var eventEntity = await _context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => new EventPostModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    CancellationReason = e.CancellationReason,
                    Stationar = e.Stationar != null ? new StationarDto
                    {
                        StationarId = e.Stationar.StationarId,
                        HallTypeId = e.Stationar.HallTypeId,
                        HallTypeName = e.Stationar.HallType.Name ?? "",
                        Type = e.Stationar.Type
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

                        SelectedEmployees = _context.EmployeeRoles
                            .Where(er => er.EventId == eventId && er.RoleInPlay.PlayId == pe.PlayId)
                            .Select(er => new EmployeeRoleSelectDto
                            {
                                EmployeeId = er.EmployeeId,
                                RoleInPlayId = er.RoleInPlayId,
                                RoleName = er.RoleInPlay.Name,
                                EmployeeFullName = er.Employee.Surname + " " + er.Employee.Name
                            }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (eventEntity == null)
            {
                return NotFound();
            }

            return Ok(eventEntity);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent(EventPostModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            var validPlayEvents = model.PlayEvents.Where(pe => pe.PlayId > 0).ToList();

            if (!validPlayEvents.Any())
                return BadRequest("Добавьте хотя бы один спектакль");

            foreach (var playEvent in validPlayEvents)
            {
                if (playEvent.SelectedEmployees == null || !playEvent.SelectedEmployees.Any())
                    return BadRequest($"Для спектакля выберите состав");
            }

            try
            {
                var editedEvent = await _context.Events
                    .Include(e => e.Stationar)
                    .Include(e => e.Tour)
                    .Include(e => e.Institution)
                    .Include(e => e.EmployeeRoles)
                    .Include(e => e.PlayEvents)
                    .FirstOrDefaultAsync(e => e.EventId == model.EventId);

                if (editedEvent == null)
                    return NotFound();

                editedEvent.StartTime = model.StartTime;
                editedEvent.EndTime = model.EndTime;
                editedEvent.CancellationReason = model.CancellationReason;
                editedEvent.Type = model.Type;
                editedEvent.LastEditTime = DateTime.Now;
                editedEvent.User = currentUser;

                switch (model.Type)
                {
                    case "stationar":

                        if (model.Stationar.HallTypeId != editedEvent.Stationar.HallTypeId)
                        {
                            var hallType = await _context.HallTypes
                                .FirstOrDefaultAsync(ht => ht.HallTypeId == model.Stationar.HallTypeId);
                            editedEvent.Stationar.HallType = hallType;
                            editedEvent.Stationar.HallTypeId = model.Stationar.HallTypeId;
                        }
                        editedEvent.Stationar.Type = model.Stationar?.Type;

                        if (editedEvent.Tour != null)
                        {
                            _context.Tours.Remove(editedEvent.Tour);
                            editedEvent.Tour = null;
                        }
                        if (editedEvent.Institution != null)
                        {
                            _context.Institutions.Remove(editedEvent.Institution);
                            editedEvent.Institution = null;
                        }
                        break;

                    case "tour":
                        if (editedEvent.Tour == null)
                            editedEvent.Tour = new Tour();
                        editedEvent.Tour.Country = model.Tour?.Country;
                        editedEvent.Tour.Area = model.Tour?.Area;

                        if (editedEvent.Stationar != null)
                        {
                            _context.Stationars.Remove(editedEvent.Stationar);
                            editedEvent.Stationar = null;
                        }
                        if (editedEvent.Institution != null)
                        {
                            _context.Institutions.Remove(editedEvent.Institution);
                            editedEvent.Institution = null;
                        }
                        break;

                    case "visit":
                            var existingInstitution = await _context.Institutions
                                .FindAsync(model.Institution!.InstitutionId);

                            editedEvent.Institution = existingInstitution;
                        break;
                }

                _context.EmployeeRoles.RemoveRange(editedEvent.EmployeeRoles);
                editedEvent.EmployeeRoles.Clear();

                _context.PlayEvents.RemoveRange(editedEvent.PlayEvents);
                editedEvent.PlayEvents.Clear();

                await _context.SaveChangesAsync();

                foreach (var playEventDto in model.PlayEvents)
                {
                    if (playEventDto.PlayId > 0)
                    {
                        var newPlayEvent = new PlayEvent
                        {
                            PlayId = playEventDto.PlayId,
                            StartTime = playEventDto.StartTime,
                            EndTime = playEventDto.EndTime,
                            EventId = editedEvent.EventId
                        };

                        editedEvent.PlayEvents.Add(newPlayEvent);
                    }
                }

                await _context.SaveChangesAsync();

                var allRoleInPlays = await _context.RoleInPlays.ToListAsync();

                foreach (var playEventDto in model.PlayEvents)
                {
                    var playEvent = editedEvent.PlayEvents
                        .FirstOrDefault(pe => pe.PlayId == playEventDto.PlayId);

                    if (playEvent == null) continue;

                    foreach (var assignment in playEventDto.SelectedEmployees)
                    {
                        var roleInPlay = allRoleInPlays
                            .FirstOrDefault(r => r.RoleInPlayId == assignment.RoleInPlayId && r.PlayId == playEventDto.PlayId);

                        if (roleInPlay == null) continue;

                        var employeeRole = new EmployeeRole
                        {
                            EmployeeId = assignment.EmployeeId,
                            RoleInPlayId = assignment.RoleInPlayId,
                            EventId = editedEvent.EventId
                        };

                        _context.EmployeeRoles.Add(employeeRole);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{eventId}/cancel")]
        public async Task<IActionResult> CancelEvent(int eventId, [FromBody] CancelEventRequest request)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var canceledEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (canceledEvent == null)
            {
                return NotFound("Мероприятие не найдено");
            }

            canceledEvent.CancellationReason = request.CancellationReason;
            canceledEvent.User = currentUser;
            canceledEvent.LastEditTime = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok();
        }

        public class CancelEventRequest
        {
            public string CancellationReason { get; set; } = string.Empty;
        }


        [HttpPut("{eventId}/restore")]
        public async Task<IActionResult> RestoreEvent(int eventId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var canceledEvent = await _context.Events.Where(e => e.EventId == eventId)
                                                   .FirstOrDefaultAsync();

            canceledEvent.CancellationReason = null;
            canceledEvent.User = currentUser;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{eventId}/soft-delete")]
        public async Task<IActionResult> SoftDeleteEvent(int eventId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var deletedEvent = await _context.Events.Where(e => e.EventId == eventId)
                                                   .FirstOrDefaultAsync();

            deletedEvent.DeletionTime = DateTime.Now;
            deletedEvent.User = currentUser;

            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
