using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GrotixBackend.Shared.Infrastructure.OpenApi;

/// <summary>
/// Asocia el esquema Bearer solo a operaciones con <see cref="AuthorizeAttribute"/>.
/// Sustituye un <c>AddSecurityRequirement</c> global, que hacía que Swagger marcara
/// <b>todos</b> los endpoints con requisito de JWT (y confundía el estado de los candados).
/// </summary>
/// <remarks>
/// El icono de candado en Swagger significa “este endpoint pide autenticación (JWT)”, no
/// “tienes permiso con los roles de tu token”. Un 403 por rol es solo en tiempo de ejecución.
/// </remarks>
public sealed class BearerAuthOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (IsAllowAnonymous(context)) return;
        if (!RequiresAuthentication(context)) return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            }
        ];
    }

    private static bool IsAllowAnonymous(OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            return true;
        var method = GetMethodInfo(context);
        if (method?.GetCustomAttribute<AllowAnonymousAttribute>(true) != null) return true;
        return false;
    }

    private static bool RequiresAuthentication(OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any())
            return true;
        var method = GetMethodInfo(context);
        if (method?.GetCustomAttribute<AuthorizeAttribute>(true) != null) return true;
        if (method?.DeclaringType?.GetCustomAttribute<AuthorizeAttribute>(true) != null) return true;
        return false;
    }

    private static MethodInfo? GetMethodInfo(OperationFilterContext context)
    {
        if (context.MethodInfo != null) return context.MethodInfo;
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor cad)
            return cad.MethodInfo;
        return null;
    }
}
