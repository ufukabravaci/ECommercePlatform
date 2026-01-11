using ECommercePlatform.MvcAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ECommercePlatform.MvcAdmin.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public GlobalExceptionFilter(
        ILogger<GlobalExceptionFilter> logger,
        IModelMetadataProvider modelMetadataProvider)
    {
        _logger = logger;
        _modelMetadataProvider = modelMetadataProvider;
    }

    public void OnException(ExceptionContext context)
    {
        // 1. Logla
        _logger.LogError(context.Exception, "MVC Hatası: {Message}", context.Exception.Message);

        string errorMessage = "Beklenmeyen bir hata oluştu.";

        // 2. Bağlantı Hatası Kontrolü
        if (context.Exception is HttpRequestException || context.Exception.InnerException is System.Net.Sockets.SocketException)
        {
            errorMessage = "API sunucusuna ulaşılamıyor. Lütfen daha sonra tekrar deneyiniz.";
        }
        else
        {
            errorMessage = context.Exception.Message;
        }

        var result = new ViewResult { ViewName = "Error" };

        // 4. Model'i oluştur (ErrorMessage ekleyerek)
        var errorModel = new ErrorViewModel
        {
            RequestId = context.HttpContext.TraceIdentifier,
            ErrorMessage = errorMessage  // HATA MESAJINI MODELE EKLE
        };

        result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
        result.ViewData.Model = errorModel;

        context.Result = result;
        context.ExceptionHandled = true;
    }
}