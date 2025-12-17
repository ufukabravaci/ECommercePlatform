using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ECommercePlatform.WebAPI;

internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        if (!document.Components.SecuritySchemes.ContainsKey("Bearer"))
        {
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Authorization: Bearer {token}"
            };
        }

        document.Security ??= new List<OpenApiSecurityRequirement>();

        document.Security.Add(new OpenApiSecurityRequirement
        {
            // referenceId = "Bearer", document = document, description = null
            [new OpenApiSecuritySchemeReference("Bearer", document, null)] = new List<string>()
        });

        return Task.CompletedTask;
    }
}