using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Companies;
using ECommercePlatform.Domain.Constants;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Companies;

[Permission(PermissionConsts.UpdateShippingSettings)]
public sealed record UpdateShippingSettingsCommand(
    decimal FreeShippingThreshold,
    decimal FlatRate
) : IRequest<Result<string>>; // Update olduğu için string mesaj dönmek yeterli

public sealed class UpdateShippingSettingsCommandValidator : AbstractValidator<UpdateShippingSettingsCommand>
{
    public UpdateShippingSettingsCommandValidator()
    {
        RuleFor(x => x.FreeShippingThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FlatRate).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateShippingSettingsCommandHandler(
    IRepository<Company> companyRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext
) : IRequestHandler<UpdateShippingSettingsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateShippingSettingsCommand request, CancellationToken cancellationToken)
    {
        // TenantId ile şirketi buluyoruz
        var company = await companyRepository.GetByExpressionWithTrackingAsync(
            c => c.Id == tenantContext.CompanyId,
            cancellationToken);

        if (company is null)
            return Result<string>.Failure("Şirket bulunamadı.");

        company.UpdateShippingSettings(request.FreeShippingThreshold, request.FlatRate);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Succeed("Kargo ayarları güncellendi.");
    }
}
