using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.ConflictChecker;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.DTOs.Employees;
using DocumentFormat.OpenXml.InkML;

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
                    IsCanceled = model.IsCanceled,
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
                            Hall = model.Stationar.Hall,
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
                            CastId = playEventDto.CastId > 0 ? playEventDto.CastId : null
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
                .Include(e => e.EmployeeRoles) // Добавили Include
                .Select(e => new EventGetModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    LastEditTime = e.LastEditTime,
                    IsCanceled = e.IsCanceled,
                    Plays = e.PlayEvents.Select(p => new PlayDto { Name = p.Play.Name }).ToList(),
                    Stationar = e.Stationar != null ? new StationarDto
                    {
                        StationarId = e.Stationar.StationarId,
                        Hall = e.Stationar.Hall,
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
                    EmployeeRoles = e.EmployeeRoles.Select(er => er.EmployeeId).ToList()
                })
                .ToListAsync();

            // Просто проверяем и отмечаем HasConflict
            SimpleCheckConflicts(events);

            return Ok(events);
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
                .Select(e => new EventGetModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    LastEditTime = e.LastEditTime,
                    IsCanceled = e.IsCanceled,
                    Plays = e.PlayEvents.Select(p => new PlayDto { Name = p.Play.Name }).ToList(),
                    Stationar = e.Stationar != null ? new StationarDto { Hall = e.Stationar.Hall } : null,
                    EmployeeRoles = e.EmployeeRoles.Select(er => er.EmployeeId).ToList()
                })
                .ToListAsync();

            SimpleCheckConflicts(events);

            return Ok(events);
        }

        private void SimpleCheckConflicts(List<EventGetModel> events)
        {
            foreach (var ev in events)
            {
                ev.HasConflict = false;
            }

            var hallEvents = events.Where(e => e.Type == "stationar" && e.Stationar != null).ToList();
            for (int i = 0; i < hallEvents.Count; i++)
            {
                for (int j = i + 1; j < hallEvents.Count; j++)
                {
                    if (hallEvents[i].EventId != hallEvents[j].EventId &&
                        hallEvents[i].Stationar?.Hall == hallEvents[j].Stationar?.Hall &&
                        hallEvents[i].StartTime < hallEvents[j].EndTime &&
                        hallEvents[j].StartTime < hallEvents[i].EndTime)
                    {
                        hallEvents[i].HasConflict = true;
                        hallEvents[j].HasConflict = true;
                    }
                }
            }

            var employeeGroups = events
                .SelectMany(e => e.EmployeeRoles.Select(empId => new { Event = e, EmployeeId = empId }))
                .Where(x => x.EmployeeId > 0)
                .GroupBy(x => x.EmployeeId);

            foreach (var group in employeeGroups)
            {
                var empEvents = group.ToList();
                for (int i = 0; i < empEvents.Count; i++)
                {
                    for (int j = i + 1; j < empEvents.Count; j++)
                    {
                        if (empEvents[i].Event.EventId != empEvents[j].Event.EventId &&
                            empEvents[i].Event.StartTime < empEvents[j].Event.EndTime &&
                            empEvents[j].Event.StartTime < empEvents[i].Event.EndTime)
                        {
                            empEvents[i].Event.HasConflict = true;
                            empEvents[j].Event.HasConflict = true;
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
                .Select(e => new EventConflictPreview
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    Hall = e.Stationar != null ? e.Stationar.Hall : null,
                    EmployeeIds = e.EmployeeRoles.Select(er => er.EmployeeId).ToList()
                })
                .ToListAsync();

            // Исключаем текущее мероприятие при редактировании
            if (request.ExcludeEventId.HasValue)
            {
                existingEvents = existingEvents.Where(e => e.EventId != request.ExcludeEventId.Value).ToList();
            }

            // Проверка залов
            if (request.Type == "stationar" && !string.IsNullOrEmpty(request.Hall))
            {
                var hallConflicts = existingEvents
                    .Where(e => e.Type == "stationar" && e.Hall == request.Hall &&
                                IsConflicting(request.StartTime, request.EndTime, e.StartTime, e.EndTime))
                    .Select(e => $"{request.Hall} уже занят {e.StartTime:dd.MM HH:mm} - {e.EndTime:HH:mm}")
                    .ToList();

                foreach (var conflict in hallConflicts)
                {
                    warnings.Add(new Warning { Message = conflict, Type = ConflictType.HallConflict });
                }
            }

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
                .Select(e => new EventGetModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    LastEditTime = e.LastEditTime,
                    IsCanceled = e.IsCanceled,
                    UserFullName = (e.User != null ? e.User.Surname + " " + e.User.Name + " " + e.User.FatherName : ""),
                    Stationar = e.Stationar != null ? new StationarDto
                    {
                        StationarId = e.Stationar.StationarId,
                        Hall = e.Stationar.Hall,
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
                    
                    Plays = e.PlayEvents.Select(pe => new PlayDto
                    {
                        PlayId = pe.Play.PlayId,
                        Name = pe.Play.Name,
                        Duration = pe.Play.Duration,
                        AgeCategory = pe.Play.AgeCategory
                    }).ToList()
                })
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
                    IsCanceled = e.IsCanceled,
                    Stationar = e.Stationar != null ? new StationarDto
                    {
                        StationarId = e.Stationar.StationarId,
                        Hall = e.Stationar.Hall,
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
                editedEvent.IsCanceled = model.IsCanceled;
                editedEvent.Type = model.Type;
                editedEvent.LastEditTime = DateTime.Now;
                editedEvent.User = currentUser;

                switch (model.Type)
                {
                    case "stationar":
                        if (editedEvent.Stationar == null)
                            editedEvent.Stationar = new Stationar();
                        editedEvent.Stationar.Hall = model.Stationar?.Hall;
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
                        if (editedEvent.Institution == null)
                            editedEvent.Institution = new Institution();
                        editedEvent.Institution.Name = model.Institution?.Name;
                        editedEvent.Institution.Town = model.Institution?.Town;
                        editedEvent.Institution.Street = model.Institution?.Street;
                        editedEvent.Institution.House = model.Institution?.House;

                        if (editedEvent.Stationar != null)
                        {
                            _context.Stationars.Remove(editedEvent.Stationar);
                            editedEvent.Stationar = null;
                        }
                        if (editedEvent.Tour != null)
                        {
                            _context.Tours.Remove(editedEvent.Tour);
                            editedEvent.Tour = null;
                        }
                        break;
                }

                // Удаление старых связей
                _context.EmployeeRoles.RemoveRange(editedEvent.EmployeeRoles);
                await _context.SaveChangesAsync();
                editedEvent.EmployeeRoles.Clear();

                // Создание новых связей
                foreach (var playEventDto in model.PlayEvents)
                {
                    foreach (var assignment in playEventDto.SelectedEmployees)
                    {
                        var roleInPlay = await _context.RoleInPlays
                            .FirstOrDefaultAsync(r => r.RoleInPlayId == assignment.RoleInPlayId && r.PlayId == playEventDto.PlayId);

                        var employeeRole = new EmployeeRole
                        {
                            EmployeeId = assignment.EmployeeId,
                            RoleInPlayId = assignment.RoleInPlayId,
                            Event = editedEvent
                        };

                        editedEvent.EmployeeRoles.Add(employeeRole);
                    }
                }

                // Обновление связанных спектаклей
                foreach (var playEventDto in model.PlayEvents)
                {
                    var existingPlayEvent = editedEvent.PlayEvents
                        .FirstOrDefault(pe => pe.PlayEventId == playEventDto.PlayEventId);

                    if (existingPlayEvent != null)
                    {
                        existingPlayEvent.StartTime = playEventDto.StartTime;
                        existingPlayEvent.EndTime = playEventDto.EndTime;
                    }
                    else if (playEventDto.PlayEventId == 0 && playEventDto.PlayId > 0)
                    {
                        editedEvent.PlayEvents.Add(new PlayEvent
                        {
                            PlayId = playEventDto.PlayId,
                            StartTime = playEventDto.StartTime,
                            EndTime = playEventDto.EndTime
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }


        [HttpPut("{eventId}/cancel")]
        public async Task<IActionResult> CancelEvent(int eventId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var canceledEvent = await _context.Events.Where(e => e.EventId == eventId)
                                                   .FirstOrDefaultAsync();

            canceledEvent.IsCanceled = true;
            canceledEvent.User = currentUser;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{eventId}/restore-cancel")]
        public async Task<IActionResult> RestoreCancelEvent(int eventId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var canceledEvent = await _context.Events.Where(e => e.EventId == eventId)
                                                   .FirstOrDefaultAsync();

            canceledEvent.IsCanceled = false;
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
