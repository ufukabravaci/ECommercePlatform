using ECommercePlatform.Application.Categories;
using ECommercePlatform.Domain.Categories;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Categories;

public class DeleteCategoryHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DeleteCategoryHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CategoryNotFound()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());

        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Silinecek kategori bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CategoryHasSubCategories()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());
        var category = new Category("Test", Guid.NewGuid());

        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Alt kategorileri var
        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kategori silinemiyor. Alt kategoriler mevcut.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_And_DeleteCategory_When_NoSubCategoriesExist()
    {
        var command = new DeleteCategoryCommand(Guid.NewGuid());
        var category = new Category("Test", Guid.NewGuid());

        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Alt kategorisi yok
        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().Be("Kategori başarıyla silindi.");

        // Entity'nin silindiğini (Soft delete) doğrula
        category.IsDeleted.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
