using ECommercePlatform.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommercePlatform.Infrastructure.BackgroundJobs;

public sealed class TokenCleanupService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TokenCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TokenCleanupService başlatıldı. İlk temizlik için 5dk bekleniyor...");

        try
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
        catch (TaskCanceledException) { return; } // Uygulama kapanırsa hata vermesin

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));

        do
        {
            try
            {
                await CleanTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token temizleme döngüsünde hata.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanTokensAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Token temizliği başlıyor...");

        using var scope = serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Silinecek kriterler: Expire olmuşlar VEYA Revoke edilip üzerinden 2 gün geçmiş olanlar.
        var thresholdDate = DateTimeOffset.Now.AddDays(-2);

        // ExecuteDeleteAsync doğrudan SQL çalıştırır, RAM'i şişirmez.
        var deletedCount = await context.UserRefreshTokens
            .Where(t => t.Expiration < thresholdDate || (t.RevokedAt != null && t.RevokedAt < thresholdDate))
            .ExecuteDeleteAsync(stoppingToken);

        logger.LogInformation("{Count} adet eski token temizlendi.", deletedCount);
    }
}
