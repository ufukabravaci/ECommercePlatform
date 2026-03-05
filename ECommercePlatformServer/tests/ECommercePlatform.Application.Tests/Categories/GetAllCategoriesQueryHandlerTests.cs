using ECommercePlatform.Application.Categories;
using ECommercePlatform.Domain.Categories;
using FluentAssertions;
using GenericRepository;
using Mapster; // Mapster eklendi
using MockQueryable;
using Moq;

namespace ECommercePlatform.Application.Tests.Categories;

public class GetAllCategoriesQueryHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
    private readonly GetAllCategoriesQueryHandler _handler;

    public GetAllCategoriesQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IRepository<Category>>();
        _handler = new GetAllCategoriesQueryHandler(_categoryRepositoryMock.Object);

        // ÖNEMLİ: Mapster'a test ortamında da aynı kuralları geçerli kılıyoruz!
        TypeAdapterConfig<Category, CategoryDto>
            .NewConfig()
            .Map(dest => dest.ParentName, src => src.Parent != null ? src.Parent.Name : "-");
    }

    // Entity Id atama
    private void SetEntityId(Category category, Guid id)
    {
        var property = typeof(ECommercePlatform.Domain.Abstractions.Entity).GetProperty("Id");
        property?.SetValue(category, id);
    }

    // Navigation property atama (Parent)
    private void SetParent(Category child, Category parent)
    {
        var property = typeof(Category).GetProperty("Parent");
        property?.SetValue(child, parent);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategories_OrderedByName()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        // Mapster'ın hata vermemesi için Parent'ı dolu bir senaryo hazırlayalım.
        var parentCategory = new Category("AnaKategori", companyId);
        SetEntityId(parentCategory, Guid.NewGuid());

        var cat1 = new Category("Bilgisayar", companyId); SetEntityId(cat1, Guid.NewGuid());
        var cat2 = new Category("Telefon", companyId); SetEntityId(cat2, Guid.NewGuid());
        var cat3 = new Category("Aksesuar", companyId); SetEntityId(cat3, Guid.NewGuid());

        // Birine parent atayalım (Örneğin Bilgisayar, AnaKategori'nin altında olsun)
        SetParent(cat1, parentCategory);

        // cat2 ve cat3'ün Parent nesnesi bilerek null bırakıldı. Mapster'ın ternary (?:) operatörü çalışacak.

        var list = new List<Category> { cat1, cat2, cat3 };

        _categoryRepositoryMock.Setup(x => x.GetAll()).Returns(list.BuildMock());

        var query = new GetAllCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Count.Should().Be(3);

        // A -> B -> T olarak sıralanmalı
        result.Data[0].Name.Should().Be("Aksesuar");
        result.Data[0].ParentName.Should().Be("-"); // Null olduğu için konfigürasyon çalışmalı

        result.Data[1].Name.Should().Be("Bilgisayar");
        result.Data[1].ParentName.Should().Be("AnaKategori"); // Parent set edildiği için gelmeli

        result.Data[2].Name.Should().Be("Telefon");
    }
}
