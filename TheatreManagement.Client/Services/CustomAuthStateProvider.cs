using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TheatreManagement.Shared;
using TheatreManagement.Shared.DTOs.Users;
using TheatreManagement.Client.Helpers;


namespace TheatreManagement.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ISyncLocalStorageService _localStorage;
        private Timer? _refreshTimer;
        private const int RefreshIntervalMinutes = 10;

        public CustomAuthStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage)
        {
            this._httpClient = httpClient;
            this._localStorage = localStorage;

            var accessToken = localStorage.GetItem<string>("accessToken");
            if (accessToken != null)
            {
                this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                StartRefreshTimer();
            }
        }


public override async Task<AuthenticationState> GetAuthenticationStateAsync()
{
    var user = new ClaimsPrincipal(new ClaimsIdentity());

    try
    {
        var currentUser = await GetCurrentUserAsync();
        
        if (currentUser != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, currentUser.Email),
                new(ClaimTypes.Email, currentUser.Email)
            };
            
            if (!string.IsNullOrEmpty(currentUser.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, currentUser.Role));
            }

            var identity = new ClaimsIdentity(claims, "Token");
            user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
    }
    catch (Exception)
    {
    }

    return new AuthenticationState(user);
}

        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", new {email, password});
                
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();

                    _localStorage.SetItem("accessToken", accessToken);
                    _localStorage.SetItem("refreshToken", refreshToken);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    StartRefreshTimer();
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    return new FormResult { Succeeded = true };
                }
                else
                {
                    return new FormResult { Succeeded = false, Errors = ["Неверный логин или пароль"] };
                }
            }
            catch { }

            return new FormResult {Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public void Logout()
        {
            _localStorage.RemoveItem("accessToken");
            _localStorage.RemoveItem("refreshToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<FormResult> RegisterAsync (RegisterModel registerModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Account/register", registerModel);
                return new FormResult {Succeeded = true};
            }
            catch { }

            return new FormResult {Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public async Task<FormResult> UpdateUserAsync(UserDto userDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/Account", userDto);
                return new FormResult { Succeeded = true };
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public async Task<FormResult> SoftDeleteUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Account/{userId}/soft-delete", new { });
                return new FormResult { Succeeded = true };
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public async Task<FormResult> RestoreUserAsync(string userId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Account/{userId}/restore", new { });
                return new FormResult { Succeeded = true };
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Account/current");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PagedResult<UserDto>> GetUsersPagedAsync(int page = 1, int pageSize = 10, string? searchText = null, bool isArchive = false)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<UserDto>>(
                $"api/Account?page={page}&pageSize={pageSize}&searchText={searchText}&isArchive={isArchive}");
        }

        public async Task<UserDto> GetUserByIdAsync( string userId)
        {
            return await _httpClient.GetFromJsonAsync<UserDto>(
                $"api/Account/{userId}"); 
        }

        public async Task<string?> GetCurrentUserRoleAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Role;
        }

        private async Task RefreshTokenAsync()
        {
            try
            {
                var refreshToken = _localStorage.GetItem<string>("refreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    Logout();
                    return;
                }

                var response = await _httpClient.PostAsJsonAsync("refresh", new { refreshToken });

                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var newAccessToken = jsonResponse?["accessToken"]?.ToString();
                    var newRefreshToken = jsonResponse?["refreshToken"]?.ToString();

                    if (!string.IsNullOrEmpty(newAccessToken))
                    {
                        _localStorage.SetItem("accessToken", newAccessToken);
                        _localStorage.SetItem("refreshToken", newRefreshToken);

                        _httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", newAccessToken);

                        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                    }
                }
            }
            catch
            {
            }
        }

        //Таймер для обновления токена
        private void StartRefreshTimer()
        {
            _refreshTimer?.Dispose();
            _refreshTimer = new Timer(async _ => await RefreshTokenAsync(),
                null, TimeSpan.FromMinutes(RefreshIntervalMinutes), TimeSpan.FromMinutes(RefreshIntervalMinutes));
        }
    }

}

