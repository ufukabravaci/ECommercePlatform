using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Categories;

[Permission(PermissionConsts.UpdateCategory)]
public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    Guid? ParentId
) : IRequest<Result<string>>;

public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kategori Id boş olamaz.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı boş olamaz.")
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.ParentId)
            .Must((command, parentId) =>
                parentId == null || parentId != command.Id)
            .WithMessage("Kategori kendisinin üst kategorisi olamaz.");
    }
}

public sealed class UpdateCategoryCommandHandler(
    IRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Kategoriyi Getir (Tracking Aktif, çünkü değişiklik yapacağız - tenant filtresi otomatik)
        var category = await categoryRepository.GetByExpressionWithTrackingAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null) return Result<string>.Failure("Kategori bulunamadı.");

        // 2. İsim Güncelleme
        if (category.Name != request.Name)
        {
            bool nameExists = await categoryRepository.AnyAsync(
                c => c.Name == request.Name && c.Id != request.Id,
                cancellationToken
            );

            if (nameExists)
            {
                return Result<string>.Failure(
                    "Bu isimde başka bir kategori zaten mevcut."
                );
            }

            category.UpdateName(request.Name);
        }

        // Parent değişikliği var mı?
        if (request.ParentId != category.ParentId)
        {
            if (request.ParentId is null)
            {
                category.RemoveParent();
            }
            else
            {
                var newParent = await categoryRepository
                    .GetByExpressionWithTrackingAsync(
                        c => c.Id == request.ParentId.Value,
                        cancellationToken
                    );

                if (newParent is null)
                {
                    return Result<string>.Failure(
                        "Üst kategori bulunamadı."
                    );
                }

                // Max depth kontrolü
                if (CategoryTreeHelper.GetDepth(newParent) >= CategoryRules.MaxDepth)
                {
                    return Result<string>.Failure(
                        $"Kategori en fazla {CategoryRules.MaxDepth} seviye olabilir."
                    );
                }

                try
                {
                    category.SetParent(newParent);
                }
                catch (Exception ex)
                {
                    return Result<string>.Failure(ex.Message);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return "Kategori güncellendi.";
    }
}
