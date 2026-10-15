using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Chinook.MultiTenant.Api.Tenancy;

/// <summary>
/// Resolves the tenant for every request before routing runs.
/// 1. The <c>X-Tenant</c> header wins if present.
/// 2. Otherwise the first path segment is tried: <c>/eu/artists</c> resolves tenant <c>eu</c>, and the
///    segment moves to PathBase so the endpoint still matches <c>/artists</c>.
/// 3. Otherwise the request is rejected with 400 and a problem details body naming the header.
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next, ITenantRegistry registry)
{
    public const string HeaderName = "X-Tenant";

    // Paths that do not belong to any tenant.
    private static readonly PathString[] TenantFreePaths = [new("/openapi")];

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (TenantFreePaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await next(context);
            return;
        }

        var header = context.Request.Headers[HeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(header))
        {
            if (!registry.TryGetTenant(header.Trim(), out var fromHeader))
            {
                await WriteProblemAsync(context, $"Unknown tenant '{header.Trim()}' in the {HeaderName} header.");
                return;
            }

            tenantContext.Current = fromHeader;
            await next(context);
            return;
        }

        if (TryGetFirstSegment(context.Request.Path, out var segment, out var remainder)
            && registry.TryGetTenant(segment, out var fromPath))
        {
            tenantContext.Current = fromPath;
            context.Request.PathBase = context.Request.PathBase.Add("/" + segment);
            context.Request.Path = remainder;
            await next(context);
            return;
        }

        await WriteProblemAsync(
            context,
            $"No tenant was specified. Send the {HeaderName} header (for example '{HeaderName}: us') or prefix the path with the tenant (for example '/us/artists').");
    }

    private static bool TryGetFirstSegment(PathString path, out string segment, out PathString remainder)
    {
        segment = string.Empty;
        remainder = PathString.Empty;
        var value = path.Value;
        if (string.IsNullOrEmpty(value) || value.Length < 2)
        {
            return false;
        }

        var end = value.IndexOf('/', 1);
        segment = end < 0 ? value[1..] : value[1..end];
        remainder = end < 0 ? new PathString("/") : new PathString(value[end..]);
        return segment.Length > 0;
    }

    private static Task WriteProblemAsync(HttpContext context, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Tenant could not be resolved",
            Detail = detail,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        };
        problem.Extensions["header"] = HeaderName;

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return context.Response.WriteAsJsonAsync(problem, (JsonSerializerOptions?)null, "application/problem+json");
    }
}
