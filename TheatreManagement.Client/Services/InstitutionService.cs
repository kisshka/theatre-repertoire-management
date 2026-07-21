using System.Net.Http.Json;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class InstitutionService
    {
        private readonly HttpClient _httpClient;

        public InstitutionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> CreateInstitutionAsync(InstitutionDto institution)
        {
            return await _httpClient.PostAsJsonAsync("api/institutions", institution);
        }

        public async Task<HttpResponseMessage> UpdateInstitutionAsync(InstitutionDto institution)
        {
            return await _httpClient.PutAsJsonAsync("api/institutions", institution);
        }

        public async Task<PagedResult<InstitutionDto>> GetInstitutionsPagedAsync(int page = 1, int pageSize = 10, string? searchText = null)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<InstitutionDto>>(
                 $"api/institutions?page={page}&pageSize={pageSize}&searchText={searchText}");
        }       

        public async Task<InstitutionDto> GetInstitutionAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<InstitutionDto>(
                $"api/institutions/{id}");
        }

        public async Task<List<InstitutionDto>> SearchInstitutionsAsync(string searchText)
        {
            return await _httpClient.GetFromJsonAsync<List<InstitutionDto>>($"api/institutions/search?searchText={searchText}");
        }

        public async Task<InstitutionDto> GetInstitutionByNameAsync(string name)
        {
            return await _httpClient.GetFromJsonAsync<InstitutionDto>($"api/institutions/by-name?name={name}");
        }

        public async Task<List<Guide>> GetAllInstitutionTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Guide>>("api/institutions/institution-types");
        }
    }
}