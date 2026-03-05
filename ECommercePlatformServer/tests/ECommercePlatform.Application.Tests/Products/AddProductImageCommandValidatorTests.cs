using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class AddProductImageCommandValidatorTests
{
    private readonly AddProductImageCommandValidator _validator = new();

    private IFormFile CreateMockFile(string fileName, string contentType, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        return fileMock.Object;
    }

    [Fact]
    public void Validate_ShouldHaveError_When_FileIsNull()
    {
        // Dosya Null ise validator baştan patlar (NotNull çalışmalı)
        var command = new AddProductImageCommand(Guid.NewGuid(), null!, true);
        var result = _validator.TestValidate(command);

        // NotNull kontrolünden geçemediğini doğruluyoruz
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_ShouldHaveError_When_FileExtensionIsInvalid()
    {
        var file = CreateMockFile("test.pdf", "application/pdf", 1000);
        var command = new AddProductImageCommand(Guid.NewGuid(), file, true);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.File)
              .WithErrorMessage("Geçersiz dosya uzantısı.");
    }
}
