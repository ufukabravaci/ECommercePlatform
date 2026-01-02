import { HttpInterceptorFn } from "@angular/common/http";
import { environment } from "../../../environments/environment";

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('access_token');

  // Token varsa companyId zaten token içinde var. Dolayısıyla header eklemeye gerek yok.
  if (token) {
    return next(req);
  }
  //token yok yani login olmamış kullanıcının her isteğinin headerına companyId ekliyoruz.
  const tenantId = environment.defaultTenantId;
  if (tenantId) {
    const clonedReq = req.clone({
      setHeaders: {
        'X-Tenant-ID': tenantId
      }
    });
    return next(clonedReq);
  }

  return next(req);
};
