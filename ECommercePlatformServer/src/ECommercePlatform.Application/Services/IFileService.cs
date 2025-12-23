namespace ECommercePlatform.Application.Services;

public interface IFileService
{
    // Dosyayı kaydeder ve geriye dosyanın yolunu (path/url) döner.
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string folderName
        , CancellationToken cancellationToken = default);

    // Dosyayı siler
    void Delete(string path);
}
