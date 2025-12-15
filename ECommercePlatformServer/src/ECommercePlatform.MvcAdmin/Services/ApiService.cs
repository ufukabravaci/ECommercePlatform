using Newtonsoft.Json;
using System.Net.Http.Headers;
using TS.Result;

namespace ECommercePlatform.MvcAdmin.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    // Her istekte Token'ı Header'a ekle
    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<Result<T>> PostAsync<T>(string endpoint, object data)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        return await HandleResponse<T>(response);
    }

    // API'den gelen cevabı Result<T>'ye çevir
    private async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var responseData = await response.Content.ReadAsStringAsync();

        try
        {
            var result = JsonConvert.DeserializeObject<Result<T>>(responseData);

            // Deserialize edilemediyse veya null ise
            if (result == null)
                return Result<T>.Failure("API'den geçersiz cevap döndü.");

            // Result nesnesini olduğu gibi döndür (içinde success true/false bilgisi var)
            return result;
        }
        catch
        {
            // JSON parse hatası veya API 500 patlamış html dönmüş olabilir
            return Result<T>.Failure($"Sunucu hatası: {response.StatusCode}");
        }
    }
}
