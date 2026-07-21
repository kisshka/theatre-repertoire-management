using System.Net.Http.Json;
using TheatreManagement.Client.Helpers;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class EventService
    {
        private readonly HttpClient _httpClient;

        public EventService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<FormResult> CreateEventAsync(EventPostModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/events", model);

                if (response.IsSuccessStatusCode)
                    return new FormResult { Succeeded = true };

                var errorText = await response.Content.ReadAsStringAsync();

                return new FormResult
                {
                    Succeeded = false,
                    Errors = [string.IsNullOrEmpty(errorText) ? "Ошибка сервера" : errorText]
                };
            }
            catch
            {
                return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
            }
        }

        public async Task<FormResult> UpdateEventAsync(EventPostModel model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/events", model);

                if (response.IsSuccessStatusCode)
                    return new FormResult { Succeeded = true };

                var errorText = await response.Content.ReadAsStringAsync();

                return new FormResult
                {
                    Succeeded = false,
                    Errors = [string.IsNullOrEmpty(errorText) ? "Ошибка сервера" : errorText]
                };
            }
            catch
            {
                return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
            }
        }

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

        public async Task CancelEventAsync(int eventId, string cancellationReason)
        {
            await _httpClient.PutAsJsonAsync($"api/events/{eventId}/cancel", new { cancellationReason } );
        }
        public async Task RestoreCancelEventAsync(int eventId)
        {
            await _httpClient.PutAsJsonAsync($"api/events/{eventId}/restore-cancel", new { });
        }

        public async Task SoftDeleteEventAsync(int eventId)
        {
            await _httpClient.PutAsJsonAsync($"api/events/{eventId}/soft-delete", new {});
        }


        public async Task<List<Guide>> GetAllStationarTypesAsync()
        {
           return await _httpClient.GetFromJsonAsync<List<Guide>>($"api/events/hall-types");
        }

        public async Task<PagedResult<EventGetModel>> GetEventsPagedAsync(
            int page = 1,
            int pageSize = 10,
            string? searchText = null,
            bool isArchive = false)
        {
            var url = $"api/events/paged?page={page}&pageSize={pageSize}&isArchive={isArchive}";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                url += $"&searchText={Uri.EscapeDataString(searchText)}";
            }
            return await _httpClient.GetFromJsonAsync<PagedResult<EventGetModel>>(url) ?? new PagedResult<EventGetModel>();
        }

        public async Task RestoreEventAsync(int eventId)
        {
            await _httpClient.PutAsJsonAsync($"api/events/{eventId}/restore", new { });
        }
    }
}
