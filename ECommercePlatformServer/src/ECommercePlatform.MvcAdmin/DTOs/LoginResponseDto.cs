namespace ECommercePlatform.MvcAdmin.DTOs;

public record LoginResponseDto(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    bool RequiresEmailConfirmation,
    string? Message);