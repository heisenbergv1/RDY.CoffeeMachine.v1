using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Extensions;

/// <summary>
/// Middleware helpers for Swagger / SwaggerUI.
/// </summary>
public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Swagger and SwaggerUI in Development environment.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <param name="version">OpenAPI version id (default "v1").</param>
    /// <param name="routePrefix">Route prefix for the UI (default "swagger").</param>
    /// <param name="displayName">Display name in the UI dropdown.</param>
    public static IApplicationBuilder UseAppSwagger(
        this IApplicationBuilder app,
        string version = "v1",
        string routePrefix = "swagger",
        string? displayName = null)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    $"/swagger/{version}/swagger.json",
                    displayName ?? $"API {version}");

                options.RoutePrefix = routePrefix;
            });
        }

        return app;
    }
}
