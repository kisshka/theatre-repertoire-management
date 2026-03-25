using AntDesign;
using System.Net.Http.Json;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs;

namespace TheatreManagement.Client.Services
{
    public class PlayService
    {
        private readonly HttpClient _httpClient;

        public PlayService (HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> CreatePlayAsync(PlayDto play)
        {
            return await _httpClient.PostAsJsonAsync("api/plays", play);
        }

        public async Task<PagedResult<PlayDto>> GetPlaysPagedAsync(int page = 1, int pageSize = 10, string? searchText = null)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<PlayDto>>(
                $"api/plays?page={page}&pageSize={pageSize}&searchText={searchText}");
        }

        public async Task<PlayDto> GetPlayAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PlayDto>(
                $"api/plays/{id}");
        }

    }
}
