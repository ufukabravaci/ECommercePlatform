using ECommercePlatform.Application.Categories;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Categories;
using FluentAssertions;
using GenericRepository;
using Moq;

namespace ECommercePlatform.Application.Tests.Categories;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantContextMock = new Mock<ITenantContext>();

        _handler = new CreateCategoryCommandHandler(
            _categoryRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantContextMock.Object
        );
    }

    // Entity Id'sini Reflection ile set eden yardımcı metod
    // Entity sınıfında Id property'si private set olduğu için, testlerde yeni bir Category oluşturduğumuzda
    // Id'sini manuel olarak set edemiyoruz.
    // Bu helper metod, testlerde oluşturduğumuz Category nesnelerine Id atamak için kullanılıyor.
    // parent - child ilişkisi kurarken Id'ler üzerinden eşleştirme yapmamız gerektiği durumlarda bu metod işimize yarıyor.
    private void SetEntityId(Category category, Guid id)
    {
        var property = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        property?.SetValue(category, id);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_CompanyIdIsNull()
    {
        _tenantContextMock.Setup(x => x.CompanyId).Returns((Guid?)null);
        var command = new CreateCategoryCommand("Elektronik", null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Şirket bilgisi tespit edilemedi. Lütfen tekrar giriş yapın.");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_NameAlreadyExists()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateCategoryCommand("Elektronik", null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain("Bu isimde bir kategori zaten mevcut.");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_When_RootCategoryCreated()
    {
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        _categoryRepositoryMock
            .Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateCategoryCommand("Elektronik", null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccessful.Should().BeTrue();

        _categoryRepositoryMock.Verify(x => x.AddAsync(It.Is<Category>(c => c.Name == "Elektronik" && c.ParentId == null), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_When_MaxDepthExceeded()
    {
        // CategoryRules.MaxDepth = 4 olarak tanımlı. Biz 4. seviyede bir kategori oluşturup, ona yeni bir tane daha (5. seviye) eklemeye çalışacağız.
        var companyId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.CompanyId).Returns(companyId);

        var root = new Category("Root", companyId); SetEntityId(root, Guid.NewGuid());
        var level2 = new Category("Level2", companyId); SetEntityId(level2, Guid.NewGuid()); level2.SetParent(root);
        var level3 = new Category("Level3", companyId); SetEntityId(level3, Guid.NewGuid()); level3.SetParent(level2);
        var level4 = new Category("Level4", companyId); SetEntityId(level4, Guid.NewGuid()); level4.SetParent(level3);

        // Name duplicate yok
        _categoryRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Kullanıcı level4'ün altına eklemek istiyor.
        var command = new CreateCategoryCommand("Level5", level4.Id);

        // İlk parent bulma çağrısı (GetByExpressionWithTrackingAsync) -> Level4 döner
        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionWithTrackingAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(level4);

        // while döngüsü içindeki diğer parent çağrıları (GetByExpressionAsync)
        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionAsync(It.Is<System.Linq.Expressions.Expression<Func<Category, bool>>>(expr => expr.Compile()(level3)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(level3);

        _categoryRepositoryMock
            .Setup(x => x.GetByExpressionAsync(It.Is<System.Linq.Expressions.Expression<Func<Category, bool>>>(expr => expr.Compile()(level2)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(level2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessages.Should().Contain($"Kategori en fazla {CategoryRules.MaxDepth} seviye olabilir.");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
