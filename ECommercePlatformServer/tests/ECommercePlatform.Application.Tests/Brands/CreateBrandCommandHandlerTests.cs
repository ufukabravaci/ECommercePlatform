using ECommercePlatform.Application.Brands;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Brands;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Brands;

public class CreateBrandCommandHandlerTests
{
    private readonly Mock<IRepository<Brand>> _brandRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly CreateBrandCommandHandler _handler;

    public CreateBrandCommandHandlerTests()
    {
        _brandRepositoryMock = new Mock<IRepository<Brand>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new CreateBrandCommandHandler(
            _brandRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_BrandNameAlreadyExists()
    {
        var command = new CreateBrandCommand("Nike", "logo.png");
        _brandRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu isimde bir marka zaten mevcut.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_BrandIsNew()
    {
        var command = new CreateBrandCommand("Nike", "logo.png");
        var companyId = Guid.NewGuid();

        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);
        _brandRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Brand, bool>>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        _brandRepositoryMock.Verify(x => x.Add(It.Is<Brand>(b => b.Name == "Nike" && b.CompanyId == companyId)), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
