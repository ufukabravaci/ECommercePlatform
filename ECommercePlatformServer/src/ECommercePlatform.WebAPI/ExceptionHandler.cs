using ECommercePlatform.Application.Behaviors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TS.Result;

namespace ECommercePlatform.WebAPI;

public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // innerexception
        var actualException = exception is AggregateException agg && agg.InnerException != null
            ? agg.InnerException
            : exception;

        httpContext.Response.ContentType = "application/json";

        // JSON Seçenekleri (CamelCase + Türkçe Karakter Desteği)
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // 3. Hata Tipine Göre Yönetim
        object responseResult;

        switch (actualException)
        {
            case ValidationException validationException:
                httpContext.Response.StatusCode = 422;
                responseResult = Result<string>.Failure(
                    validationException.Errors.Select(e => e.ErrorMessage).ToList()
                );
                break;

            case AuthorizationException:
                httpContext.Response.StatusCode = 403;
                responseResult = Result<string>.Failure("Bu işlem için yetkiniz bulunmamaktadır.");
                break;

            case UnauthorizedAccessException:
                httpContext.Response.StatusCode = 401;
                responseResult = Result<string>.Failure("Oturum açmanız gerekmektedir.");
                break;

            case ArgumentException argEx: // Domain validation hataları
                httpContext.Response.StatusCode = 400;
                responseResult = Result<string>.Failure(argEx.Message);
                break;

            // Diğer Custom Exception'lar buraya (örn: NotFoundException)

            default:
                _logger.LogError(actualException, "Beklenmeyen hata: {Message}", actualException.Message);

                httpContext.Response.StatusCode = 500;
                responseResult = Result<string>.Failure("Sunucu tarafında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                break;
        }

        await httpContext.Response.WriteAsJsonAsync(responseResult, jsonOptions, cancellationToken);
        return true;
    }
}
