using ECommercePlatform.Application.Categories;
using ECommercePlatform.Domain.Categories;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Categories;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new UpdateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // Yardımcı metod
    private void SetEntityId(Category category, Guid id)
    {
        var property = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        property?.SetValue(category, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CategoryNotFound()
    {
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Yeni İsim", null);

        _categoryRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync((Category)null!);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Kategori bulunamadı.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_NewNameAlreadyExistsInAnotherCategory()
    {
        var catId = Guid.NewGuid();
        var category = new Category("Eski İsim", Guid.NewGuid());
        SetEntityId(category, catId);

        var command = new UpdateCategoryCommand(catId, "Yeni İsim", null);

        _categoryRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(category);

        // Yeni isim başka bir kategoride zaten var!
        _categoryRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu isimde başka bir kategori zaten mevcut.");
    }

    [Fact]
    public async Task Handle_ShouldUpdateNameAndRemoveParent_When_ParentIdIsNull()
    {
        var catId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        // Başlangıçta bir parent'ı var
        var parentCat = new Category("Parent", companyId); SetEntityId(parentCat, Guid.NewGuid());
        var category = new Category("Eski İsim", companyId); SetEntityId(category, catId);
        category.SetParent(parentCat); // Parent bağlı

        // Kullanıcı ParentId = null göndererek ana kategori yapmak istiyor
        var command = new UpdateCategoryCommand(catId, "Yeni İsim", null);

        _categoryRepositoryMock.Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(category);

        // Yeni isim çakışmıyor
        _categoryRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();
        category.Name.Should().Be("Yeni İsim");
        category.ParentId.Should().BeNull(); // Parent silinmiş olmalı!

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
