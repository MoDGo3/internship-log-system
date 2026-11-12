using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Storage;

namespace InternLog.API.Tests.Infrastructure;

public sealed class FakeCurrentUser : ICurrentUserService
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = "Test";
    public bool IsAuthenticated => UserId != 0;

    public FakeCurrentUser As(int userId, string role)
    {
        UserId = userId;
        Role = role;
        return this;
    }
}

public sealed class FakeWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = Path.GetTempPath();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ApplicationName { get; set; } = "InternLog.API.Tests";
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Testing";
}

/// <summary>LocalFileStorage บนโฟลเดอร์ชั่วคราว — ลบทิ้งเมื่อ Dispose</summary>
public sealed class TempFileStorage : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "internlog-tests", Guid.NewGuid().ToString("N"));
    public LocalFileStorage Storage { get; }

    public TempFileStorage()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FileStorage:Root"] = Root })
            .Build();
        Storage = new LocalFileStorage(config, new FakeWebHostEnvironment());
    }

    public void Dispose()
    {
        if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
    }
}

public static class TestFiles
{
    public static readonly byte[] PdfHeader = Encoding.ASCII.GetBytes("%PDF-1.7\n");

    public static IFormFile Create(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName);
    }

    public static IFormFile Pdf(string fileName = "doc.pdf", int extraBytes = 100)
        => Create(fileName, [.. PdfHeader, .. new byte[extraBytes]]);
}
