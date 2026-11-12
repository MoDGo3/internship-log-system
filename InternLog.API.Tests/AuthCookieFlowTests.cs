using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using InternLog.API.Tests.Infrastructure;

namespace InternLog.API.Tests;

/// <summary>
/// ทดสอบผ่าน HTTP จริง: refresh token อยู่ใน httpOnly cookie และไฟล์อัปโหลดไม่ถูกเสิร์ฟแบบ static
/// </summary>
public class AuthCookieFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthCookieFlowTests(ApiFactory factory) => _factory = factory;

    private static Task<HttpResponseMessage> Login(HttpClient client) =>
        client.PostAsJsonAsync("/api/auth/login", new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });

    private static string? RefreshCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith("refreshToken=", StringComparison.Ordinal))
            : null;

    [Fact]
    public async Task Login_SetsHttpOnlyRefreshCookie_AndBodyHasNoRefreshToken()
    {
        var client = _factory.CreateClient();

        var response = await Login(client);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = RefreshCookie(response);
        Assert.NotNull(cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", cookie, StringComparison.OrdinalIgnoreCase);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = body.RootElement.GetProperty("data");
        Assert.True(data.TryGetProperty("accessToken", out _));
        Assert.False(data.TryGetProperty("refreshToken", out _));
    }

    [Fact]
    public async Task Refresh_UsesCookie_AndRotatesIt()
    {
        var client = _factory.CreateClient(); // จัดการ cookie ให้อัตโนมัติ
        var login = await Login(client);

        var refresh = await client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.NotEqual(RefreshCookie(login), RefreshCookie(refresh));
    }

    [Fact]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        var client = _factory.CreateClient();

        var refresh = await client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesSession_SoRefreshFails()
    {
        var client = _factory.CreateClient();
        await Login(client);

        var logout = await client.PostAsync("/api/auth/logout", null);
        var refresh = await client.PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Theory]
    [InlineData("/uploads/leaves/anything.pdf")]
    [InlineData("/uploads/reports/Report_1.pdf")]
    public async Task OldStaticUploadPaths_AreNotServed(string path)
    {
        var response = await _factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/leaves/1/attachment")]
    [InlineData("/api/worklogs/attachments/1")]
    [InlineData("/api/reports/1/export-pdf")]
    public async Task DownloadEndpoints_RequireLogin(string path)
    {
        var response = await _factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
