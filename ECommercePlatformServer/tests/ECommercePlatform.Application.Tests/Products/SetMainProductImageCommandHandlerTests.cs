using ECommercePlatform.Application.Products;
using ECommercePlatform.Domain.Products;
using ECommercePlatform.Domain.Shared;
using FluentAssertions;
using GenericRepository;
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Products;

public class SetMainProductImageCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SetMainProductImageCommandHandler _handler;

    public SetMainProductImageCommandHandlerTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new SetMainProductImageCommandHandler(_productRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldSetMainImage_When_Valid()
    {
        var product = new Product("Laptop", "SKU456", "Açıklama", new Money(10, Currency.TRY), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        product.AddImage("resim1.jpg", true);  // Bu main
        product.AddImage("resim2.jpg", false); // Bu değil

        var resim2Id = product.Images.First(x => x.ImageUrl == "resim2.jpg").Id;

        _productRepoMock.Setup(x => x.WhereWithTracking(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
                .Returns(new List<Product> { product }.BuildMock());

        var command = new SetMainProductImageCommand(Guid.NewGuid(), resim2Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        // 2. resim artık main olmalı
        product.Images.First(x => x.ImageUrl == "resim2.jpg").IsMain.Should().BeTrue();
        product.Images.First(x => x.ImageUrl == "resim1.jpg").IsMain.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
