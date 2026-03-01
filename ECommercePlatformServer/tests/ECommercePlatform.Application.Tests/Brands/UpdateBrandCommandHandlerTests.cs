using ECommercePlatform.Application.Brands;
using ECommercePlatform.Domain.Brands;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Brands;

public class UpdateBrandCommandHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateBrandCommandHandler _handler;

    public UpdateBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateBrandCommandHandler(_brandRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BrandNotFound()
    {
        var command = new UpdateBrandCommand(Guid.NewGuid(), "New Name", null);
        _brandRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Brand)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Marka bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_NewNameAlreadyExistsInAnotherBrand()
    {
        var brand = new Brand("Old Name", null, Guid.NewGuid());
        var command = new UpdateBrandCommand(Guid.NewGuid(), "Existing Name", null);

        _brandRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(brand);

        // İsim değişiyor ve yeni isim DB'de var
        _brandRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu isimde başka bir marka zaten var.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateBrand_When_Valid()
    {
        var brand = new Brand("Old Name", null, Guid.NewGuid());
        var command = new UpdateBrandCommand(Guid.NewGuid(), "New Name", "new-logo.png");

        _brandRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(brand);

        _brandRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        brand.Name.Should().Be("New Name");
        brand.LogoUrl.Should().Be("new-logo.png");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
