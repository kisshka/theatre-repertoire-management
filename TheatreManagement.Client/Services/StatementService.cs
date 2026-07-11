using System.Net.Http.Json;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs.Availability;

namespace TheatreManagement.Client.Services
{
    public class StatementService
    {
        private readonly HttpClient _httpClient;

        public StatementService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedResult<PlayAvailabilityDto>> GetPlayAvailabilityAsync(
            DateTime startTime,
            DateTime endTime,
            int page = 1,
            int pageSize = 6,
            string? searchText = null)
        {
            var url = $"api/availability/plays?startTime={startTime:yyyy-MM-ddTHH:mm:ss}&endTime={endTime:yyyy-MM-ddTHH:mm:ss}&page={page}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchText))
                url += $"&searchText={Uri.EscapeDataString(searchText)}";

            var result = await _httpClient.GetFromJsonAsync<PagedResult<PlayAvailabilityDto>>(url);
            return result ?? new PagedResult<PlayAvailabilityDto>();
        }
    }
}