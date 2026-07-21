using System.Net.Http.Json;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class HallTypeService
    {
        private readonly HttpClient _httpClient;

        public HallTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Guide>> GetHallTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Guide>>("api/HallTypes");
        }

        public async Task AddHallTypeAsync(string name)
        {
            var response = await _httpClient.PostAsJsonAsync("api/HallTypes", new Guide { Name = name });
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateHallTypeAsync(int id, string name)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/HallTypes/{id}", new Guide { Name = name });
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteHallTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/HallTypes/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}