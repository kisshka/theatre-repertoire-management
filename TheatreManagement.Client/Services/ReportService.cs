using Microsoft.AspNetCore.Components;

namespace TheatreManagement.Client.Services
{
    public class ReportService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navManager;

        public ReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ReportResult> ExportEventsToExcelAsync(
                    DateTime startDate,
                    DateTime endDate,
                    string? type = null,
                    int? employeeId = null)
        {
            try
            {
                var url = $"api/Reports/events-report?start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}";

                if (!string.IsNullOrWhiteSpace(type))
                {
                    url += $"&type={Uri.EscapeDataString(type)}";
                }

                if (employeeId.HasValue && employeeId.Value > 0)
                {
                    url += $"&employeeId={employeeId.Value}";
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return ReportResult.Fail($"Ошибка сервера: {error}");
                }

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Events_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                _navManager.NavigateTo(url, true);

                return ReportResult.Success(fileName);
            }
            catch (Exception ex)
            {
                return ReportResult.Fail($"Ошибка при экспорте: {ex.Message}");
            }
        }


        // В ReportService.cs
        public string GetReportUrl(DateTime startDate, DateTime endDate, string? type = null)
        {
            var url = $"api/Reports/events-report?start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}";

            if (!string.IsNullOrWhiteSpace(type))
            {
                url += $"&type={Uri.EscapeDataString(type)}";
            }

            return url;
        }
    }


    public class ReportResult
    {
        public bool IsSuccess { get; set; }
        public string? FileName { get; set; }
        public string? ErrorMessage { get; set; }

        public static ReportResult Success(string fileName) => new()
        {
            IsSuccess = true,
            FileName = fileName
        };

        public static ReportResult Fail(string errorMessage) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }


}
