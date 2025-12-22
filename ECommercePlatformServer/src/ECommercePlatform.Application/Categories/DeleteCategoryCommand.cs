using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Domain.Categories;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Categories;

[Permission(PermissionConsts.DeleteCategory)]
public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result<string>>;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty().WithMessage("Kategori ID bilgisi boş olamaz.");
    }
}

public sealed class DeleteCategoryHandler(
    IRepository<Category> categoryRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteCategoryCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByExpressionWithTrackingAsync(c => c.Id == request.Id, cancellationToken);
        if (category is null)
        {
            return Result<string>.Failure("Silinecek kategori bulunamadı.");
        }
        bool hasSubCategories = await categoryRepository.AnyAsync(c => c.ParentId == request.Id, cancellationToken);
        if (hasSubCategories)
        {
            return Result<string>.Failure("Kategori silinemiyor. Alt kategoriler mevcut.");
        }
        category.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return "Kategori başarıyla silindi.";
    }
}