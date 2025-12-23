using ECommercePlatform.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace ECommercePlatform.Infrastructure.Services;

public sealed class LocalFileService(IWebHostEnvironment environment) : IFileService
{
    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folderName, CancellationToken cancellationToken = default)
    {
        // 1. Klasör Yolu: wwwroot/uploads/companyId/products
        string uploadPath = Path.Combine(environment.WebRootPath, "uploads", folderName);
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

        // 2. Dosya Adı (Güvenlik için uzantı kontrolü yapılmalı)
        string extension = Path.GetExtension(fileName);
        string uniqueName = $"{Guid.NewGuid()}{extension}";
        string fullPath = Path.Combine(uploadPath, uniqueName);

        // 3. Kayıt (Stream kopyalama)
        using var targetStream = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(targetStream, cancellationToken);

        // 4. URL Dönüşü
        return $"/uploads/{folderName}/{uniqueName}".Replace("\\", "/");
    }

    public void Delete(string path)
    {
        // Path: /uploads/companyId/products/resim.jpg
        if (string.IsNullOrEmpty(path)) return;

        // Baştaki slash'ı kaldırıp tam yolu bul
        string fullPath = Path.Combine(environment.WebRootPath, path.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}