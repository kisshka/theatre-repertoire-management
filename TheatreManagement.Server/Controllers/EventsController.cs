using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TheatreManagement.Domain.Data;
using TheatreManagement.Domain.Entities;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Events;

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

            var newEvent = new Event
            {
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                LastEditTime = DateTime.Now,
                IsCanceled = model.IsCanceled,
                User = currentUser,
                Type = model.Type
            };

            // Специфичные данные
            switch (model.Type)
            {
                case "stationar":
                    var stationar = new Stationar
                    {
                        Hall = model.Stationar.Hall,
                        Type = model.Stationar.Type
                    };
                    _context.Stationars.Add(stationar);
                    newEvent.Stationar = stationar;
                    break;

                case "tour":
                    var tour = new Tour
                    {
                        Country = model.Tour.Country,
                        Area = model.Tour.Area
                    };
                    _context.Tours.Add(tour);
                    newEvent.Tour = tour;
                    break;

                case "visit":
                    var institution = new Institution
                    {
                        Name = model.Institution.Name,
                        Town = model.Institution.Town,
                        Street = model.Institution.Street,
                        House = model.Institution.House
                    };
                    _context.Institutions.Add(institution);
                    newEvent.Institution = institution;
                    break;
            }

            // Связи
            foreach (var playEvent in model.PlayEvents)
            {
                if (playEvent.PlayId > 0)
                {
                    newEvent.PlayEvents.Add(new PlayEvent
                    {
                        PlayId = playEvent.PlayId,
                        StartTime = playEvent.StartTime,
                        EndTime = playEvent.EndTime
                    });
                }
            }

            // Связи с ролями и сотрудниками
            if (model.EmployeeRolesCreate != null && model.EmployeeRolesCreate.Any())
            {
                foreach (var employeeRoleCreate in model.EmployeeRolesCreate)
                {
                    // Проверяем существование сотрудника
                    var employee = await _context.Employees
                        .FindAsync(employeeRoleCreate.EmployeeId);
                    if (employee == null)
                        return BadRequest($"Сотрудник с ID {employeeRoleCreate.EmployeeId} не найден");

                    // Проверяем существование роли
                    var roleInPlay = await _context.RoleInPlays
                        .FindAsync(employeeRoleCreate.RoleInPlayId);
                    if (roleInPlay == null)
                        return BadRequest($"Роль с ID {employeeRoleCreate.RoleInPlayId} не найдена");

                    var employeeRole = new EmployeeRole
                    {
                        EmployeeId = employeeRoleCreate.EmployeeId,
                        RoleInPlayId = employeeRoleCreate.RoleInPlayId,
                        Event = newEvent,
                        //CastId = employeeRoleCreate.CastId
                    };

                    _context.EmployeeRoles.Add(employeeRole);
                }
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{date}")]
        public async Task<ActionResult<List<EventGetModel>>> GetEventsByDate(DateTime date)
        {
            var startTime = date.Date;
            var nextDay = startTime.AddDays(1);

            // Ищем мероприятия, диапазон которых пересекается с выбранным днем
            var events = await _context.Events
                .Where(e => e.StartTime < nextDay && e.EndTime > startTime
                )
                .OrderBy(e => e.StartTime)
                .Include(e => e.PlayEvents)
                .Select(e => new EventGetModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    LastEditTime = e.LastEditTime,
                    IsCanceled = e.IsCanceled,
                    Plays = e.PlayEvents.Where(p => p.EventId == e.EventId )
                                        .Select(p => new PlayDto
                                        {
                                           Name = p.Play.Name
                                        }).ToList(),
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
                    } : null
                })
                .ToListAsync();



            return Ok(events);
        }

        [HttpGet("range")]
        public async Task<ActionResult<List<EventGetModel>>> GetEventsByDateRange(  [FromQuery] string start,
                                                                                    [FromQuery] string end)
        {
            if (!DateTime.TryParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                return BadRequest("Неверный формат даты");
            }

            var events = await _context.Events
                .Where(e => e.StartTime < endDate.AddDays(1) && e.EndTime > startDate)
                .Include(e => e.PlayEvents)
                .OrderBy(e => e.StartTime)
                .Select(e => new EventGetModel
                {
                    EventId = e.EventId,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Type = e.Type,
                    LastEditTime = e.LastEditTime,
                    IsCanceled = e.IsCanceled,
                    Plays = e.PlayEvents.Where(p => p.EventId == e.EventId)
                                        .Select(p => new PlayDto
                                        {
                                            Name = p.Play.Name
                                        }).ToList(),
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
                    } : null
                })
                .ToListAsync();
             
            return Ok(events);
        }

        [HttpGet("{eventId}/details")]
        public async Task<ActionResult<EventGetModel>> GetEventDetails(int eventId)
        {
            var eventEntity = await _context.Events
                .Where(e => e.EventId == eventId)
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

            // Получаем EmployeeRoles со связанными данными
            var employeeRoles = await _context.EmployeeRoles
                .Where(er => er.EventId == eventId)
                .Include(er => er.Employee)
                .Include(er => er.RoleInPlay)
                            .ThenInclude(c => c.Play)
                .Include(er => er.Cast)
                .ToListAsync();

            var playsWithRoles = employeeRoles
                //Группировка по спектаклю
                .GroupBy(er => new { er.RoleInPlay.Play.PlayId, er.RoleInPlay.Play.Name })
                .Select(playGroup => new PlayWithRolesDto
                {
                    PlayId = playGroup.Key.PlayId,
                    PlayName = playGroup.Key.Name,

                    //Группировка по ролям
                    RoleGroups = playGroup
                        .GroupBy(er => er.RoleInPlayId)
                        .Select(roleGroup => new RoleGroupDto
                        {
                            RoleInPlayId = roleGroup.Key,
                            RoleName = roleGroup.First().RoleInPlay?.Name,
                            RoleType = roleGroup.First().RoleInPlay?.Type,
                            Employees = roleGroup
                                .Select(er => new EmployeeDto
                                {
                                    EmployeeId = er.EmployeeId,
                                    Surname = er.Employee?.Surname,
                                    Name = er.Employee?.Name,
                                    FatherName = er.Employee?.FatherName,
                                    Post = er.Employee?.Post
                                })
                                .DistinctBy(e => e.EmployeeId)
                                .ToList()
                        }).ToList()
                })
                .ToList();

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

                        SelectedEmployees = new Dictionary<int, List<int>>()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (eventEntity == null)
            {
                return NotFound();
            }

            var employeeRoles = await _context.EmployeeRoles
                .Where(er => er.EventId == eventId)
                .Include(er => er.Employee)
                .Include(er => er.RoleInPlay)
                .ToListAsync();


            foreach (var playEvent in eventEntity.PlayEvents)
            {
                var rolesForPlayEvent = employeeRoles
                    .Where(er => er.EventId == eventId)
                    .GroupBy(er => er.RoleInPlayId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(er => er.EmployeeId).ToList()
                    );

                playEvent.SelectedEmployees = rolesForPlayEvent;

                // Восстанавливаем отображение ролей
                await RestoreRoleDisplays(playEvent);
            }

            return Ok(eventEntity);
        }

        private async Task RestoreRoleDisplays(PlayEventDto playEvent)
        {
            playEvent.RoleDisplays.Clear();

            foreach (var role in playEvent.SelectedEmployees)
            {
                int roleId = role.Key;
                var employeeIds = role.Value;

                // Получаем название роли
                var roleInfo = await _context.RoleInPlays
                    .Where(r => r.RoleInPlayId == roleId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();

                var employeeNames = new List<string>();

                foreach (var empId in employeeIds.Where(id => id > 0))
                {
                    var employee = await _context.Employees
                        .Where(e => e.EmployeeId == empId)
                        .Select(e => $"{e.Surname} {e.Name}")
                        .FirstOrDefaultAsync();

                    if (employee != null)
                        employeeNames.Add(employee);
                }

                playEvent.RoleDisplays.Add(new PlayEventRoleDisplay
                {
                    RoleName = roleInfo,
                    EmployeeNames = string.Join(", ", employeeNames),
                    Count = employeeIds.Count
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEvent(EventPostModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized("Пользователь не авторизован");
            }

            var editedEvent = await _context.Events.Where(e=> e.EventId == model.EventId)
                                                   .Include(e => e.Stationar)
                                                   .Include(e => e.Tour)
                                                   .Include(e => e.Institution)
                                                   .Include(e => e.EmployeeRoles)
                                                   .Include(e => e.PlayEvents)
                                                   .FirstOrDefaultAsync();

            editedEvent.StartTime = model.StartTime;
            editedEvent.EndTime = model.EndTime;
            editedEvent.IsCanceled = model.IsCanceled;
            editedEvent.Type = model.Type;
            editedEvent.LastEditTime = DateTime.Now;

            // Специфичные данные
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
            _context.PlayEvents.RemoveRange(editedEvent.PlayEvents);

            // Связи
            foreach (var playEvent in model.PlayEvents)
            {
                if (playEvent.PlayId > 0)
                {
                    editedEvent.PlayEvents.Add(new PlayEvent
                    {
                        PlayId = playEvent.PlayId,
                        StartTime = playEvent.StartTime,
                        EndTime = playEvent.EndTime
                    });
                }
            }

            // Связи с ролями и сотрудниками
            if (model.EmployeeRolesCreate != null && model.EmployeeRolesCreate.Any())
            {
                foreach (var employeeRoleCreate in model.EmployeeRolesCreate)
                {
                    // Проверяем существование сотрудника
                    var employee = await _context.Employees
                        .FindAsync(employeeRoleCreate.EmployeeId);
                    if (employee == null)
                        return BadRequest($"Сотрудник с ID {employeeRoleCreate.EmployeeId} не найден");

                    // Проверяем существование роли
                    var roleInPlay = await _context.RoleInPlays
                        .FindAsync(employeeRoleCreate.RoleInPlayId);
                    if (roleInPlay == null)
                        return BadRequest($"Роль с ID {employeeRoleCreate.RoleInPlayId} не найдена");

                    var employeeRole = new EmployeeRole
                    {
                        EmployeeId = employeeRoleCreate.EmployeeId,
                        RoleInPlayId = employeeRoleCreate.RoleInPlayId,
                        Event = editedEvent,
                        //CastId = employeeRoleCreate.CastId
                    };

                    _context.EmployeeRoles.Add(employeeRole);
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
