using Microsoft.OpenApi;

namespace Extensions;

/// <summary>
/// Swagger / OpenAPI registration helpers shared across services.
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// Registers Swagger / OpenAPI with a standard setup.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="apiTitle">Title to show in Swagger UI.</param>
    /// <param name="version">OpenAPI version id (default "v1").</param>
    public static IServiceCollection AddSwagger(
        this IServiceCollection services,
        string apiTitle,
        string version = "v1")
    {
        // NOTE: We assume AddEndpointsApiExplorer() is called in the API project.
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = apiTitle,
                Version = version
            });

            // Standard JWT bearer config so Swagger has an Authorize button.
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            };

            options.AddSecurityDefinition("bearer", securityScheme);

            // IMPORTANT:
            // Your Microsoft.OpenApi version defines OpenApiSecurityRequirement as a
            // Dictionary<OpenApiSecuritySchemeReference, List<string>>.
            // Our previous collection initializer tried to use:
            //   key: OpenApiSecurityScheme
            //   value: string[]
            // which does not match and caused the compiler error.
            //
            // To avoid guessing the exact reference type/constructor and keep things compiling,
            // we register an *empty* requirement. The "Bearer" scheme is still defined above,
            // so the Authorize button appears in Swagger UI; we just don't force a global
            // requirement here.
            var requirement = new OpenApiSecurityRequirement();

            // In your Swashbuckle version, AddSecurityRequirement expects a Func<OpenApiDocument, OpenApiSecurityRequirement>,
            // not a raw OpenApiSecurityRequirement instance. We therefore pass a lambda that always returns the same requirement.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        return services;
    }
}
