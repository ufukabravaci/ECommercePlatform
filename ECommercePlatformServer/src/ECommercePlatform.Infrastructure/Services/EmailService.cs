using ECommercePlatform.Application.Services;
using FluentEmail.Core;

namespace ECommercePlatform.Infrastructure.Services;

internal sealed class MailService(IFluentEmail fluentEmail) : IEmailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var sendResponse = await fluentEmail.To(to).Subject(subject).Body(body, true).SendAsync(cancellationToken);

        if (!sendResponse.Successful)
        {
            throw new ArgumentException(string.Join(", ", sendResponse.ErrorMessages));
        }
    }
}
