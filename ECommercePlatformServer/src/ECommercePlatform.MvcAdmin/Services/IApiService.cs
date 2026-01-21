using TS.Result;

namespace ECommercePlatform.MvcAdmin.Services;

public interface IApiService
{
    Task<Result<T>> GetAsync<T>(string endpoint);
    Task<Result<T>> PostAsync<T>(string endpoint, object data);
    Task<Result<T>> PutAsync<T>(string endpoint, object data);
    Task<Result<T>> DeleteAsync<T>(string endpoint);
    Task<Result<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content);
    Task<Result<T>> PatchAsync<T>(string endpoint, object? data = null);
    Task<Result<T>> PutMultipartAsync<T>(string endpoint, MultipartFormDataContent content);
}
