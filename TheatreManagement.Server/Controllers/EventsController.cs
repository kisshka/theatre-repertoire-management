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
                    newEvent.Institution = new Institution
                    {
                        Name = model.Institution.Name,
                        Town = model.Institution.Town,
                        Street = model.Institution.Street,
                        House = model.Institution.House
                    };
                    break;
            }

            // Добавляем PlayEvents (только спектакли, без ролей)
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

            // Добавляем EmployeeRoles (роли сотрудников, привязанные только к Event)
            foreach (var playEventDto in model.PlayEvents)
            {
                foreach (var selectedEmployee in playEventDto.SelectedEmployees)
                {
                    if (selectedEmployee.EmployeeId <= 0) continue;

                    // Проверяем существование сотрудника
                    var employee = await _context.Employees.FindAsync(selectedEmployee.EmployeeId);
                    if (employee == null)
                        return BadRequest($"Сотрудник с ID {selectedEmployee.EmployeeId} не найден");

                    // Проверяем существование роли
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
                                //.DistinctBy(e => e.EmployeeId)
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
