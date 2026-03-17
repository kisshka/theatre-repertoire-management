using System.Net.Http.Json;
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
        public async Task<HttpResponseMessage> CreatePlayAsync(PlayDTO play)
        {
            return await _httpClient.PostAsJsonAsync("api/plays", play);
        }

        public async Task<List<PlayDTO>> GetAllPlaysAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PlayDTO>>("api/plays");
        }

        public async Task<PlayDTO> GetPlayByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PlayDTO>($"api/plays/{id}");
        }

        public async Task<HttpResponseMessage> UpdatePlayAsync(int id, PlayDTO play)
        {
            return await _httpClient.PutAsJsonAsync($"api/plays/{id}", play);
        }

        public async Task<HttpResponseMessage> DeletePlayAsync(int id)
        {
            return await _httpClient.DeleteAsync($"api/plays/{id}");
        }


    }
}
