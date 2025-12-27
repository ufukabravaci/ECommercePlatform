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
    private async Task AddAuthorizationHeaderAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        await context.Session.LoadAsync();
        var token = context.Session.GetString("AccessToken");
        // 1. Bearer Token Ekle
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        // 2. Tenant ID Header Ekle (YENİ)
        // Login olmuş kullanıcının Session'ında CompanyId varsa bunu Header'a bas.
        // Bu sayede API'deki TenantContext bunu okuyabilir.
        var companyId = context.Session.GetString("CompanyId");
        if (!string.IsNullOrEmpty(companyId))
        {
            // Eski header varsa sil ki çakışmasın
            if (_httpClient.DefaultRequestHeaders.Contains("X-Tenant-ID"))
                _httpClient.DefaultRequestHeaders.Remove("X-Tenant-ID");

            _httpClient.DefaultRequestHeaders.Add("X-Tenant-ID", companyId);
        }
    }

    public async Task<Result<T>> GetAsync<T>(string endpoint)
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.GetAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<Result<T>> PostAsync<T>(string endpoint, object data)
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        return await HandleResponse<T>(response);
    }

    public async Task<Result<T>> PutAsync<T>(string endpoint, object data)
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data); // System.Net.Http.Json extension
        return await HandleResponse<T>(response);
    }

    public async Task<Result<T>> DeleteAsync<T>(string endpoint)
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.DeleteAsync(endpoint);
        return await HandleResponse<T>(response);
    }

    public async Task<Result<T>> PatchAsync<T>(string endpoint, object? data = null)
    {
        await AddAuthorizationHeaderAsync();
        // PatchAsJsonAsync System.Net.Http.Json namespace'inden gelir.
        // Data null ise boş json göndeririz veya direkt request atarız.
        HttpResponseMessage response;

        if (data != null)
            response = await _httpClient.PatchAsJsonAsync(endpoint, data);
        else
            response = await _httpClient.PatchAsync(endpoint, null);

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

    public async Task<Result<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
    {
        await AddAuthorizationHeaderAsync();
        var response = await _httpClient.PostAsync(endpoint, content);
        return await HandleResponse<T>(response);
    }

}
