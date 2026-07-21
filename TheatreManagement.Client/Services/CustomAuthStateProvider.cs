using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TheatreManagement.Client.Helpers;
using TheatreManagement.Shared.DTOs.Users;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
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
                var response = await _httpClient.PostAsJsonAsync("login", new { email, password });

                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();

                    _localStorage.SetItemAsync("accessToken", accessToken);
                    _localStorage.SetItemAsync("refreshToken", refreshToken);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    return new FormResult { Succeeded = true };
                }
                else
                {
                    return new FormResult { Succeeded = false, Errors = ["Неверный логин или пароль"] };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetPasswordAsync error: {ex.Message}");
            }

            return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public void Logout()
        {
            _localStorage.RemoveItemAsync("accessToken");
            _localStorage.RemoveItemAsync("refreshToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<FormResult> RegisterAsync(RegisterModel registerModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Account/register", registerModel);
                return new FormResult { Succeeded = true };
            }
            catch { }

            return new FormResult { Succeeded = false, Errors = ["Ошибка подключения"] };
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

        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            return await _httpClient.GetFromJsonAsync<UserDto>(
                $"api/Account/{userId}");
        }

        public async Task<string?> GetCurrentUserRoleAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.Role;
        }

        public async Task<FormResult> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Account/forgot", new { email });

                if (response.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new FormResult { Succeeded = false, Errors = [error] };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ForgotPasswordAsync error: {ex.Message}");
                return new FormResult { Succeeded = false, Errors = [$"Ошибка: {ex.Message}"] };
            }
        }

        public async Task<FormResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Account/reset", new
                {
                    email,
                    token,
                    newPassword
                });

                if (response.IsSuccessStatusCode)
                {
                    return new FormResult { Succeeded = true };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new FormResult { Succeeded = false, Errors = [error] };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetPasswordAsync error: {ex.Message}");
                return new FormResult { Succeeded = false, Errors = [$"Ошибка: {ex.Message}"] };
            }
        }
    }

    // ТЕПЕРЬ ТОЧНО РАБОЧЕЕ ОБНОВЛЕНИЕ ТОКЕНА Я СКОРО УБИВАТЬ НАЧНУ
    public class AuthHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public AuthHandler(ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var refreshRequest = new HttpRequestMessage(HttpMethod.Post,
                        new Uri(request.RequestUri!, "/refresh"));

                    refreshRequest.Content = JsonContent.Create(new { refreshToken });

                    var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken);

                    if (refreshResponse.IsSuccessStatusCode)
                    {
                        var strResponse = await refreshResponse.Content.ReadAsStringAsync();
                        var jsonResponse = JsonNode.Parse(strResponse);
                        var newAccessToken = jsonResponse?["accessToken"]?.ToString();
                        var newRefreshToken = jsonResponse?["refreshToken"]?.ToString();


                            await _localStorage.SetItemAsync("accessToken", newAccessToken);
                            await _localStorage.SetItemAsync("refreshToken", newRefreshToken);

                            var retryRequest = new HttpRequestMessage(request.Method, request.RequestUri);

                            if (request.Content != null)
                            {
                                var content = await request.Content.ReadAsByteArrayAsync();
                                retryRequest.Content = new ByteArrayContent(content);
                                retryRequest.Content.Headers.ContentType = request.Content.Headers.ContentType;
                            }

                            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                            response = await base.SendAsync(retryRequest, cancellationToken);

                    }
                    else
                    {
                        await _localStorage.RemoveItemAsync("accessToken");
                        await _localStorage.RemoveItemAsync("refreshToken");
                        //_navigationManager.NavigateTo("/login", forceLoad: true);
                    }
                }
                else
                {
                    _navigationManager.NavigateTo("/login", forceLoad: false);
                }
            }

            return response;
        }
    }

}