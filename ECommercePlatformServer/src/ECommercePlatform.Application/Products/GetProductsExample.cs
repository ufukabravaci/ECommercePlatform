//using Mapster; // ProjectToType için
//using Microsoft.EntityFrameworkCore; // ToListAsync için

//public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
//{
//    // 1. IQueryable Al (Henüz DB'ye gitmedi, sadece SQL oluşuyor)
//    var query = productRepository.AsQueryable();

//    // 2. Filtreleme (SQL WHERE bloğuna eklenir)
//    // Global Filter zaten CompanyId'yi ekledi. Ekstra filtreler:
//    if (!string.IsNullOrEmpty(request.Search))
//    {
//        query = query.Where(p => p.Name.Contains(request.Search));
//    }

//    // 3. Projection (SQL SELECT bloğunu optimize eder)
//    // Sadece DTO'da olan kolonları çeker (SELECT Name, Price FROM Products...)
//    // Entity'yi memory'e alıp maplemek yerine, SQL'den direkt DTO döner.
//    var dtoQuery = query.ProjectToType<ProductDto>();

//    // 4. Pagination (SQL OFFSET/FETCH)
//    // Skip ve Take, SQL'de sayfalama yapar.
//    int skip = (request.PageNumber - 1) * request.PageSize;
//    var pagedResult = await dtoQuery
//        .Skip(skip)
//        .Take(request.PageSize)
//        .ToListAsync(cancellationToken); // <--- SQL BURADA ÇALIŞIR

//    return pagedResult;
//}