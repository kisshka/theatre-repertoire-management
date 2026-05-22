using Microsoft.AspNetCore.Components;

namespace TheatreManagement.Client.Services
{
    public class ReportService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navManager;
        private readonly IConfiguration _configuration;

        public ReportService(HttpClient httpClient, NavigationManager navManager, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _navManager = navManager;
            _configuration = configuration;
        }

        public string GetReportUrl(DateTime startDate, DateTime endDate, string? type = null, bool includeCast = false, int? employeeId = null)
        {

            var apiBaseUrl = _configuration["WebApiAdress"];

            var url = $"{apiBaseUrl}/api/Reports/events-report?start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}";

            if (!string.IsNullOrWhiteSpace(type))
            {
                url += $"&type={type}";
            }

            if (includeCast)
            {
                url += $"&includeCast=true";
            }

            if (employeeId.HasValue && employeeId > 0)
            {
                url += $"&employeeId={employeeId}";
            }

            return url;
        }
    }

}
