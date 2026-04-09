using System.Net.Http.Json;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.DTOs.Events;

namespace TheatreManagement.Client.Services
{
    public class EventService
    {
        private readonly HttpClient _httpClient;

        public EventService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateEventAsync(EventPostModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/events", model);
            response.EnsureSuccessStatusCode();
        }

        // В EventService.cs
        public async Task<List<EventGetModel>> GetEventsByDayAsync(DateTime date)
        {
            var dateString = date.ToString("yyyy-MM-dd");
            return await _httpClient.GetFromJsonAsync<List<EventGetModel>>($"api/events/{dateString}");
        }

        public async Task<List<EventGetModel>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var start = startDate.ToString("yyyy-MM-dd");
            var end = endDate.ToString("yyyy-MM-dd");
            return await _httpClient.GetFromJsonAsync<List<EventGetModel>>($"api/events/range?start={start}&end={end}");
        }

        public async Task<EventGetModel> GetEventDetailsAsync(int eventId)
        {
            return await _httpClient.GetFromJsonAsync<EventGetModel>($"api/events/{eventId}/details");
        }

        public async Task<EventPostModel> GetEventForEditAsync(int eventId)
        {
            return await _httpClient.GetFromJsonAsync<EventPostModel>($"api/events/{eventId}/for-edit");
        }

        public async Task UpdateEventAsync(EventPostModel model)
        {
            await _httpClient.PutAsJsonAsync($"api/events", model);
        }

    }
}
