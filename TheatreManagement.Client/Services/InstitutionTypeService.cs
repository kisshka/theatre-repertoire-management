using System.Net.Http.Json;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class InstitutionTypeService
    {
        private readonly HttpClient _httpClient;

        public InstitutionTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Guide>> GetInstitutionTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Guide>>("api/InstitutionTypes") ?? new();
        }

        public async Task AddInstitutionTypeAsync(string name)
        {
            var response = await _httpClient.PostAsJsonAsync("api/InstitutionTypes", new Guide { Name = name });
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateInstitutionTypeAsync(int id, string name)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/InstitutionTypes/{id}", new Guide { Name = name });
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteInstitutionTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/InstitutionTypes/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
