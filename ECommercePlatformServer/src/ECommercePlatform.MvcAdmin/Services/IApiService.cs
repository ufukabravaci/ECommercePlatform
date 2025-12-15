using TS.Result;

namespace ECommercePlatform.MvcAdmin.Services;

public interface IApiService
{
    Task<Result<T>> GetAsync<T>(string endpoint);
    Task<Result<T>> PostAsync<T>(string endpoint, object data);
}
