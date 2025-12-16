namespace ECommercePlatform.MvcAdmin.Models;

public record ConfirmEmailViewModel
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}
