using ECommercePlatform.Application.Attributes;
using ECommercePlatform.Application.Services;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Users;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace ECommercePlatform.Application.Employee;

[Permission(PermissionConsts.ManagePermissions)]
public sealed record UpdateEmployeePermissionCommand(
    Guid UserId,      // GetEmployeesQuery'den gelen UserId buraya basılacak
    string Permission,
    bool IsGranted    // true: Ver, false: Al
) : IRequest<Result<string>>;

public sealed class UpdateEmployeePermissionCommandHandler(
    ICompanyUserRepository companyUserRepository,
    ITenantContext tenantContext,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateEmployeePermissionCommand, Result<string>>
{
    public async Task<Result<string>> Handle(UpdateEmployeePermissionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.CompanyId is null)
            return Result<string>.Failure("Şirket oturumu bulunamadı.");

        // 1. Kullanıcıyı Şirket Context'inde Bul
        // "Bana UserID'si bu olan ve BENİM şirketimde çalışan kişiyi getir"
        var targetCompanyUser = await companyUserRepository.FirstOrDefaultAsync(
            x => x.UserId == request.UserId && x.CompanyId == tenantContext.CompanyId,
            cancellationToken);

        if (targetCompanyUser is null)
            return Result<string>.Failure("Çalışan bulunamadı veya bu şirkete ait değil.");

        // 2. Yetkiyi İşle
        if (request.IsGranted)
        {
            targetCompanyUser.AddPermission(request.Permission);
        }
        else
        {
            targetCompanyUser.RemovePermission(request.Permission);
        }

        // 3. Kaydet
        companyUserRepository.Update(targetCompanyUser);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string action = request.IsGranted ? "verildi" : "geri alındı";
        return Result<string>.Succeed($"Yetki başarıyla {action}.");
    }
}