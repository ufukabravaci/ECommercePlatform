namespace ECommercePlatform.MvcAdmin.Models;

public record TwoFactorLoginViewModel
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}
