using System.Text.Json;
using Chinook.MultiTenant.Api.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Chinook.MultiTenant.Tests;

public class TenantResolutionMiddlewareTests
{
    /// <summary>Stands in for the configuration-backed registry, so no connection string or database is involved.</summary>
    private sealed class FakeTenantRegistry : ITenantRegistry
    {
        public bool TryGetTenant(string tenantId, out Tenant tenant)
        {
            tenant = null!;
            if (tenantId.Equals("us", StringComparison.OrdinalIgnoreCase) || tenantId.Equals("eu", StringComparison.OrdinalIgnoreCase))
            {
                tenant = new Tenant(tenantId.ToLowerInvariant(), $"fake-connection-{tenantId}");
                return true;
            }

            return false;
        }
    }

    private static async Task<(HttpContext Context, TenantContext Tenant, bool NextCalled)> InvokeAsync(
        string path, string? header = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        if (header is not null)
        {
            context.Request.Headers[TenantResolutionMiddleware.HeaderName] = header;
        }

        var nextCalled = false;
        var middleware = new TenantResolutionMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, new FakeTenantRegistry());
        var tenantContext = new TenantContext();

        await middleware.InvokeAsync(context, tenantContext);
        return (context, tenantContext, nextCalled);
    }

    [Fact]
    public async Task Resolves_tenant_from_header()
    {
        var (context, tenant, nextCalled) = await InvokeAsync("/artists", header: "us");

        Assert.True(nextCalled);
        Assert.Equal("us", tenant.Current?.Id);
        Assert.Equal("/artists", context.Request.Path.Value);
    }

    [Fact]
    public async Task Resolves_tenant_from_first_path_segment_and_strips_it()
    {
        var (context, tenant, nextCalled) = await InvokeAsync("/eu/artists");

        Assert.True(nextCalled);
        Assert.Equal("eu", tenant.Current?.Id);
        Assert.Equal("/eu", context.Request.PathBase.Value);
        Assert.Equal("/artists", context.Request.Path.Value);
    }

    [Fact]
    public async Task Header_wins_over_path()
    {
        var (_, tenant, _) = await InvokeAsync("/eu/artists", header: "us");

        Assert.Equal("us", tenant.Current?.Id);
    }

    [Fact]
    public async Task Missing_tenant_returns_400_problem_details_naming_the_header()
    {
        var (context, tenant, nextCalled) = await InvokeAsync("/artists");

        Assert.False(nextCalled);
        Assert.Null(tenant.Current);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(400, body.RootElement.GetProperty("status").GetInt32());
        Assert.Contains(TenantResolutionMiddleware.HeaderName, body.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Unknown_tenant_in_header_returns_400()
    {
        var (context, _, nextCalled) = await InvokeAsync("/artists", header: "apac");

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }
}
