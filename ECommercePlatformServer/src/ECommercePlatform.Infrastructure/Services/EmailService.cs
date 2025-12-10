using ECommercePlatform.Application.Services;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class MailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {

    }
}
