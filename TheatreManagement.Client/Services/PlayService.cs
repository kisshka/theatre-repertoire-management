using AntDesign;
using System.Net.Http.Json;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared.Helpers;

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
        public async Task<HttpResponseMessage> UpdatePlayAsync(PlayDto play)
        {
            return await _httpClient.PutAsJsonAsync("api/plays", play);
        }

        public async Task<HttpResponseMessage> SoftDeletePlayAsync(int playId)
        {
            return await _httpClient.PutAsJsonAsync($"api/plays/{playId}/soft-delete", new { });
        }

        public async Task<HttpResponseMessage> RestorePlayAsync(int playId)
        {
            return await _httpClient.PutAsJsonAsync($"api/plays/{playId}/restore", new { });
        }

        public async Task<PagedResult<PlayDto>> GetPlaysPagedAsync(int page = 1, int pageSize = 10, string? searchText = null, bool isArchive = false)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<PlayDto>>(
                $"api/plays?page={page}&pageSize={pageSize}&searchText={searchText}&isArchive={isArchive}");
        }

        public async Task<List<PlayDto>> GetPlaysAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PlayDto>>(
                $"api/plays/all");
        }

        public async Task<PlayDto> GetPlayAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PlayDto>(
                $"api/plays/{id}");
        }

        // Работа с составами
        public async Task<HttpResponseMessage> CreateCastAsync(CastDto castDto)
        {
            return await _httpClient.PostAsJsonAsync($"api/casts", castDto);
        }
        public async Task<HttpResponseMessage> UpdateCastAsync(CastDto updateCastDto)
        {
            return await _httpClient.PutAsJsonAsync("api/casts", updateCastDto);
        }

        public async Task<HttpResponseMessage> SoftDeleteCastAsync(int castId)
        {
            return await _httpClient.PutAsJsonAsync($"api/casts/{castId}/soft-delete", new {});
        }

        public async Task<List<CastDto>> GetCastsAsync(int playId)
        {
            return await _httpClient.GetFromJsonAsync<List<CastDto>>($"api/casts/{playId}");
        }

        public async Task<CastWithRolesDto> GetCastAsync(int castId)
        {
            if (castId > 0)
            {
                return await _httpClient.GetFromJsonAsync<CastWithRolesDto>($"api/casts/{castId}/employeeroles");
            }
            else 
                return new CastWithRolesDto ();
        }
        public async Task<List<Guide>> GetAllSceneTypeAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Guide>>($"api/plays/scene-types");
        }
    }
}
