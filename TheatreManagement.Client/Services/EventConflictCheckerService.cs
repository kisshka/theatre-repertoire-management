using System.Net.Http;
using System.Net.Http.Json;
using TheatreManagement.Shared.ConflictChecker;
using TheatreManagement.Shared.DTOs.Events;

namespace TheatreManagement.Client.Services
{
    public class EventConflictCheckerService
    {
        private readonly HttpClient _httpClient;

        public EventConflictCheckerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConflictCheckResponse> CheckConflictsAsync(ConflictCheckRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/events/check-conflicts", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ConflictCheckResponse>();
            }

            return new ConflictCheckResponse { HasConflicts = false, Warnings = new List<string>() };
        }

    }
}
