using ECommercePlatform.Application.Banners;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ECommercePlatform.Application.Tests.Banners;

public class CreateBannerCommandValidatorTests
{
    private readonly CreateBannerCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_When_TitleIsEmpty()
    {
        // IFormFile mock oluştur (Validasyon için içeriği önemli değil, sadece null olmaması yeterli)
        var fileMock = new Mock<IFormFile>();

        var command = new CreateBannerCommand(string.Empty, "Desc", fileMock.Object, "/url", 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_ImageIsNull()
    {
        // Image null gönderiliyor
        var command = new CreateBannerCommand("Test Title", "Desc", null!, "/url", 1);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Image)
              .WithErrorMessage("Banner görseli zorunludur.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_When_CommandIsValid()
    {
        var fileMock = new Mock<IFormFile>();
        var command = new CreateBannerCommand("Test Title", "Desc", fileMock.Object, "/url", 1);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
