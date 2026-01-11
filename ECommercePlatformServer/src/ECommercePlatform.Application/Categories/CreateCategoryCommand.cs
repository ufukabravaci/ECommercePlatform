using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Categories;

[Permission(PermissionConsts.CreateCategory)]
public sealed record CreateCategoryCommand(
    string Name,
    Guid? ParentId
) : IRequest<Result<Guid>>;

public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Kategori adı en az 2 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty) //null olabilir ama null değilse boş olamaz
            .WithMessage("Geçersiz üst kategori bilgisi.");
    }
}

public sealed class CreateCategoryCommandHandler(
    IRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext
) : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        Guid? companyId = tenantContext.CompanyId;
        if (!companyId.HasValue)
        {
            // Token'da CompanyId yoksa (veya Admin panelinden bağlam dışı bir işlemse)
            return Result<Guid>.Failure(500, "Şirket bilgisi tespit edilemedi. Lütfen tekrar giriş yapın.");
        }

        // İsim Kontrolü (Aynı şirkette aynı isimli kategori olamaz)
        bool isNameExists = await categoryRepository.AnyAsync(
            p => p.Name == request.Name && p.CompanyId == companyId.Value,
            cancellationToken);
        if (isNameExists)
        {
            return Result<Guid>.Failure("Bu isimde bir kategori zaten mevcut.");
        }

        Category category = new(request.Name, companyId.Value);

        // Parent Ataması
        if (request.ParentId.HasValue)
        {
            // 1. Önce seçilen Parent'ı getir (İlişki kurmak için buna ihtiyacımız var)
            var parentCategory = await categoryRepository.GetByExpressionWithTrackingAsync(
                p => p.Id == request.ParentId.Value,
                cancellationToken
            );

            if (parentCategory is null)
                return Result<Guid>.Failure("Seçilen üst kategori bulunamadı.");

            // 2. Derinliği Hesapla (While Döngüsü ile)
            // Yeni eklenecek kategori 1. seviye, Parent 2. seviye... diye sayacağız.
            // Eğer Parent varsa, derinlik en az 2'dir (Ben + Babam).
            int currentDepth = 1;
            Guid? currentParentId = parentCategory.Id;

            // Tepeye kadar tırman
            while (currentParentId.HasValue)
            {
                currentDepth++; // Bir kat daha çıktık

                if (currentDepth > CategoryRules.MaxDepth)
                {
                    return Result<Guid>.Failure($"Kategori en fazla {CategoryRules.MaxDepth} seviye olabilir.");
                }

                // Bir üstteki babayı bulmamız lazım.
                // parentCategory zaten elimizde, onun ParentId'sine bakıyoruz.
                // Ancak döngü 2. tura girdiğinde, babanın babasını DB'den çekmemiz lazım.

                if (currentDepth == 2)
                {
                    // İlk turda zaten elimizde 'parentCategory' var, tekrar DB'ye gitme.
                    currentParentId = parentCategory.ParentId;
                }
                else
                {
                    // Daha yukarı çıkıyorsak DB'den babanın babasını çek.
                    // Sadece ParentId'yi okumak için 'AsNoTracking' veya basit bir Select atabilirsin ama
                    // Repository yapısı gereği entity çekiyoruz.
                    var upperCategory = await categoryRepository.GetByExpressionAsync(
                        x => x.Id == currentParentId.Value,
                        cancellationToken);

                    currentParentId = upperCategory?.ParentId;
                }
            }

            // 3. Kontrol geçtiyse atamayı yap
            try
            {
                category.SetParent(parentCategory);
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }
        }

        // 4. Kayıt
        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Succeed(category.Id);
    }

}
