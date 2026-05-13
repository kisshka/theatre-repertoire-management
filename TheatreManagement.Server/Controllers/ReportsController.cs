using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TheatreManagement.Domain.Data;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Domain.Entities;
using Microsoft.VisualBasic;
using TheatreManagement.Shared.DTOs.Employees;

namespace TheatreManagement.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly DataContext _context;

        public ReportsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("events-report")]
        public async Task<IActionResult> ExportEventsToExcel([FromQuery] string start,
                                                            [FromQuery] string end,
                                                            [FromQuery] string? type = null,
                                                            [FromQuery] bool includeCast = false,
                                                            [FromQuery] int? employeeId = null)
        {
            if (!DateTime.TryParseExact(start, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(end, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                return BadRequest("Неверный формат даты");
            }

            // Базовый запрос
            var query = _context.Events
                .Where(e => e.StartTime < endDate.AddDays(1) && e.EndTime > startDate)
                .Include(e => e.PlayEvents)
                    .ThenInclude(pe => pe.Play)
                .Include(e => e.Stationar)
                .Include(e => e.Tour)
                .Include(e => e.Institution)
                .AsQueryable();

            // Фильтры
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(e => e.Type == type);
            }

            string employeeFullName = "";
            if (employeeId.HasValue && employeeId > 0)
            {
                query = query.Where(e => e.EmployeeRoles.Any(er => er.EmployeeId == employeeId));

                var employee = await _context.Employees
                    .Where(e => e.EmployeeId == employeeId)
                    .FirstOrDefaultAsync();
                employeeFullName = $"{employee?.Surname} {employee?.Name} {employee?.FatherName}";
            }

            var events = await query.ToListAsync();

            // Загрузка состава
            Dictionary<(int eventId, int playId), List<RoleGroupDto>> castByEventAndPlay = new();

            if (includeCast && events.Any())
            {
                var eventIds = events.Select(e => e.EventId).ToList();
                var playIds = events
                    .SelectMany(e => e.PlayEvents.Select(pe => pe.PlayId))
                    .Distinct()
                    .ToList();

                var allEmployeeRoles = await _context.EmployeeRoles
                    .Include(er => er.Employee)
                    .Include(er => er.RoleInPlay)
                    .Where(er => (er.EventId != null && eventIds.Contains(er.EventId.Value)) ||
                                 (er.RoleInPlay != null && playIds.Contains(er.RoleInPlay.PlayId)))
                    .ToListAsync();

                foreach (var er in allEmployeeRoles)
                {
                    if (er.EventId.HasValue && er.RoleInPlay != null)
                    {
                        var key = (er.EventId.Value, er.RoleInPlay.PlayId);

                        if (!castByEventAndPlay.ContainsKey(key))
                        {
                            castByEventAndPlay[key] = new List<RoleGroupDto>();
                        }

                        var roleGroup = castByEventAndPlay[key]
                            .FirstOrDefault(r => r.RoleInPlayId == er.RoleInPlayId);

                        if (roleGroup == null)
                        {
                            roleGroup = new RoleGroupDto
                            {
                                RoleInPlayId = er.RoleInPlayId,
                                RoleName = er.RoleInPlay?.Name,
                                RoleType = er.RoleInPlay?.Type,
                                Employees = new List<EmployeeDto>()
                            };
                            castByEventAndPlay[key].Add(roleGroup);
                        }

                        roleGroup.Employees.Add(new EmployeeDto
                        {
                            EmployeeId = er.EmployeeId,
                            Surname = er.Employee?.Surname ?? "",
                            Name = er.Employee?.Name ?? "",
                            FatherName = er.Employee?.FatherName ?? "",
                            Post = er.Employee?.Post ?? ""
                        });
                    }
                }
            }

            // Создание отчета
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Расписание мероприятий");

                // Заголовок
                string Title = $"Расписание мероприятий за период {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} ";
                if (type != null) Title += $"(Только {GetEventTypeRu(type)}) ";
                if (includeCast) Title += "с составом участников";

                int lastColumn = includeCast ? 8 : 6;

                worksheet.Cell(1, 1).Value = Title.Trim();
                worksheet.Range(1, 1, 1, lastColumn).Merge();
                worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                worksheet.Cell(1, 1).Style.Font.Bold = true;

                if (!string.IsNullOrEmpty(employeeFullName))
                {
                    worksheet.Cell(2, 1).Value = employeeFullName;
                    worksheet.Range(2, 1, 2, lastColumn).Merge();
                    worksheet.Cell(2, 1).Style.Font.FontSize = 14;
                }

                // Заголовки колонок
                int headerRow = string.IsNullOrEmpty(employeeFullName) ? 3 : 4;
                worksheet.Cell(headerRow, 1).Value = "Дата";
                worksheet.Cell(headerRow, 2).Value = "Время";
                worksheet.Cell(headerRow, 3).Value = "Тип";
                worksheet.Cell(headerRow, 4).Value = "Место проведения";
                worksheet.Cell(headerRow, 5).Value = "Статус";
                worksheet.Cell(headerRow, 6).Value = "Спектакль";

                if (includeCast)
                {
                    worksheet.Cell(headerRow, 7).Value = "Актерский состав";
                    worksheet.Cell(headerRow, 8).Value = "Технический состав";
                }

                var headerRange = worksheet.Row(headerRow);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Группировка данных
                var groupedEvents = new List<GroupedEventRow>();

                foreach (var ev in events.OrderBy(e => e.StartTime))
                {
                    var playNames = ev.PlayEvents.Select(p => p.Play).ToList();
                    var startEventDate = ev.StartTime.Date;
                    var endEventDate = ev.EndTime.Date;

                    if (endEventDate > startEventDate)
                    {
                        // Многодневное мероприятие
                        var days = Enumerable.Range(0, (endEventDate - startEventDate).Days + 1)
                                             .Select(offset => startEventDate.AddDays(offset))
                                             .ToList();

                        foreach (var day in days)
                        {
                            string timeRange;
                            if (day == startEventDate)
                            {
                                var endOfDay = day.AddDays(1).AddSeconds(-1);
                                timeRange = $"{ev.StartTime:HH:mm} - {endOfDay:HH:mm}";
                            }
                            else if (day == endEventDate)
                            {
                                timeRange = $"00:00 - {ev.EndTime:HH:mm}";
                            }
                            else
                            {
                                timeRange = "00:00 - 23:59";
                            }

                            var groupedRow = new GroupedEventRow
                            {
                                Date = day,
                                Event = ev,
                                TimeRange = timeRange,
                                Type = GetEventTypeRu(ev.Type),
                                Location = GetLocation(ev),
                                Status = ev.IsCanceled ? "Отменено" : "Активно",
                                Plays = playNames
                            };

                            if (includeCast)
                            {
                                foreach (var play in playNames)
                                {
                                    var key = (ev.EventId, play.PlayId);
                                    var actorCastText = "—";
                                    var techCastText = "—";

                                    if (castByEventAndPlay.TryGetValue(key, out var cast))
                                    {
                                        actorCastText = string.Join("\n", cast
                                            .Where(r => r.RoleType == "Актерская")
                                            .Select(r => $"{r.RoleName}: {(r.Employees.Any() ? string.Join(", ", r.Employees.Select(e => $"{e.Surname} {e.Name}")) : "—")}"));

                                        techCastText = string.Join("\n", cast
                                            .Where(r => r.RoleType == "Техническая")
                                            .Select(r => $"{r.RoleName}: {(r.Employees.Any() ? string.Join(", ", r.Employees.Select(e => $"{e.Surname} {e.Name}")) : "—")}"));
                                    }

                                    groupedRow.CastByPlay[play.PlayId] = (
                                        string.IsNullOrEmpty(actorCastText) ? "—" : actorCastText,
                                        string.IsNullOrEmpty(techCastText) ? "—" : techCastText
                                    );
                                }
                            }

                            groupedEvents.Add(groupedRow);
                        }
                    }
                    else
                    {
                        // Однодневное мероприятие
                        var timeRange = $"{ev.StartTime:HH:mm} - {ev.EndTime:HH:mm}";

                        var groupedRow = new GroupedEventRow
                        {
                            Date = ev.StartTime.Date,
                            Event = ev,
                            TimeRange = timeRange,
                            Type = GetEventTypeRu(ev.Type),
                            Location = GetLocation(ev),
                            Status = ev.IsCanceled ? "Отменено" : "Активно",
                            Plays = playNames
                        };

                        if (includeCast)
                        {
                            foreach (var play in playNames)
                            {
                                var key = (ev.EventId, play.PlayId);
                                var actorCastText = "—";
                                var techCastText = "—";

                                if (castByEventAndPlay.TryGetValue(key, out var cast))
                                {
                                    actorCastText = string.Join("\n", cast
                                        .Where(r => r.RoleType == "Актерская")
                                        .Select(r => $"{r.RoleName}: {(r.Employees.Any() ? string.Join(", ", r.Employees.Select(e => $"{e.Surname} {e.Name}")) : "—")}"));

                                    techCastText = string.Join("\n", cast
                                        .Where(r => r.RoleType == "Техническая")
                                        .Select(r => $"{r.RoleName}: {(r.Employees.Any() ? string.Join(", ", r.Employees.Select(e => $"{e.Surname} {e.Name}")) : "—")}"));
                                }

                                groupedRow.CastByPlay[play.PlayId] = (
                                    string.IsNullOrEmpty(actorCastText) ? "—" : actorCastText,
                                    string.IsNullOrEmpty(techCastText) ? "—" : techCastText
                                );
                            }
                        }

                        groupedEvents.Add(groupedRow);
                    }
                }

                // Вывод в Excel с группировкой по дате
                var groupedByDate = groupedEvents.GroupBy(g => g.Date).OrderBy(g => g.Key);
                int currentRow = headerRow + 1;
                int totalEvents = 0;

                foreach (var dateGroup in groupedByDate)
                {
                    var rowsInDate = dateGroup.ToList();
                    int startRow = currentRow;
                    int rowsCount = rowsInDate.Sum(r => r.Plays.Count);

                    // Заголовок даты
                    worksheet.Cell(currentRow, 1).Value = dateGroup.Key.ToString("dddd, dd MMMM yyyy");
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (rowsCount > 1)
                    {
                        worksheet.Range(currentRow, 1, currentRow + rowsCount - 1, 1).Merge();
                        worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }

                    int rowOffset = 0;

                    foreach (var groupedRow in rowsInDate)
                    {
                        int playCount = groupedRow.Plays.Count;

                        // Объединяем ячейки с общими данными
                        if (playCount > 1)
                        {
                            worksheet.Range(currentRow + rowOffset, 2, currentRow + rowOffset + playCount - 1, 2).Merge();
                            worksheet.Range(currentRow + rowOffset, 3, currentRow + rowOffset + playCount - 1, 3).Merge();
                            worksheet.Range(currentRow + rowOffset, 4, currentRow + rowOffset + playCount - 1, 4).Merge();
                            worksheet.Range(currentRow + rowOffset, 5, currentRow + rowOffset + playCount - 1, 5).Merge();

                            worksheet.Range(currentRow + rowOffset, 2, currentRow + rowOffset + playCount - 1, 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Range(currentRow + rowOffset, 3, currentRow + rowOffset + playCount - 1, 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Range(currentRow + rowOffset, 4, currentRow + rowOffset + playCount - 1, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Range(currentRow + rowOffset, 5, currentRow + rowOffset + playCount - 1, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        }

                        // Заполняем общие поля
                        worksheet.Cell(currentRow + rowOffset, 2).Value = groupedRow.TimeRange;
                        worksheet.Cell(currentRow + rowOffset, 3).Value = groupedRow.Type;
                        worksheet.Cell(currentRow + rowOffset, 4).Value = groupedRow.Location;
                        worksheet.Cell(currentRow + rowOffset, 5).Value = groupedRow.Status;

                        // Заполняем спектакли и составы
                        for (int i = 0; i < playCount; i++)
                        {
                            int targetRow = currentRow + rowOffset + i;
                            var play = groupedRow.Plays[i];

                            worksheet.Cell(targetRow, 6).Value = play.Name;

                            if (includeCast && groupedRow.CastByPlay.ContainsKey(play.PlayId))
                            {
                                var (actorCast, techCast) = groupedRow.CastByPlay[play.PlayId];
                                worksheet.Cell(targetRow, 7).Value = actorCast;
                                worksheet.Cell(targetRow, 8).Value = techCast;
                                worksheet.Cell(targetRow, 7).Style.Alignment.WrapText = true;
                                worksheet.Cell(targetRow, 8).Style.Alignment.WrapText = true;
                            }
                            else if (includeCast)
                            {
                                worksheet.Cell(targetRow, 7).Value = "—";
                                worksheet.Cell(targetRow, 8).Value = "—";
                            }

                            if (groupedRow.Event.IsCanceled)
                            {
                                worksheet.Row(targetRow).Style.Font.FontColor = XLColor.Red;
                            }

                            totalEvents++;
                        }

                        rowOffset += playCount;
                    }

                    currentRow += rowsCount;
                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                if (includeCast)
                {
                    if (worksheet.Column(7).Width < 35) worksheet.Column(7).Width = 35;
                    if (worksheet.Column(8).Width < 35) worksheet.Column(8).Width = 35;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = $"Events_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // Вспомогательные классы
        public class GroupedEventRow
        {
            public DateTime Date { get; set; }
            public Event Event { get; set; }
            public string TimeRange { get; set; }
            public string Type { get; set; }
            public string Location { get; set; }
            public string Status { get; set; }
            public List<Play> Plays { get; set; } = new();
            public Dictionary<int, (string actorCast, string techCast)> CastByPlay { get; set; } = new();
        }

        private string GetEventTypeRu(string type)
        {
            return type switch
            {
                "stationar" => "Стационар",
                "tour" => "Гастроли",
                "visit" => "Выезд",
                _ => type
            };
        }

        private string GetLocation(Event ev)
        {
            if (ev.Type == "stationar" && ev.Stationar != null)
                return $"{ev.Stationar.Hall}";
            if (ev.Type == "visit" && ev.Institution != null)
                return $"{ev.Institution.Name}";
            if (ev.Type == "tour" && ev.Tour != null)
                return $"{ev.Tour.Country} - {ev.Tour.Area}";
            return "—";
        }

    }
}
