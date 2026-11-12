using InternLog.API.Core.Common;
using InternLog.API.Tests.Infrastructure;

namespace InternLog.API.Tests;

public class FileStorageTests : IDisposable
{
    private const long OneMb = 1024 * 1024;
    private readonly TempFileStorage _temp = new();

    public void Dispose() => _temp.Dispose();

    [Fact]
    public async Task SaveUpload_ValidPdf_StoresOutsideWebRoot_AndCanBeOpened()
    {
        var path = await _temp.Storage.SaveUploadAsync(TestFiles.Pdf(), "leaves", OneMb);

        Assert.StartsWith("leaves/", path);
        Assert.EndsWith(".pdf", path);
        Assert.DoesNotContain("doc", path); // ชื่อไฟล์ของผู้ใช้ไม่ถูกใช้บน disk

        using var file = _temp.Storage.Open(path, "doc.pdf")!.Content;
        Assert.Equal(TestFiles.PdfHeader.Length + 100, file.Length);
    }

    [Fact]
    public async Task SaveUpload_ContentDoesNotMatchExtension_IsRejected()
    {
        var fakePdf = TestFiles.Create("evil.pdf", "MZ this is an exe"u8.ToArray());

        var ex = await Assert.ThrowsAsync<AppException>(() => _temp.Storage.SaveUploadAsync(fakePdf, "leaves", OneMb));
        Assert.Equal(400, ex.StatusCode);
    }

    [Theory]
    [InlineData("script.exe")]
    [InlineData("page.html")]
    [InlineData("noextension")]
    public async Task SaveUpload_DisallowedExtension_IsRejected(string fileName)
    {
        var file = TestFiles.Create(fileName, [.. TestFiles.PdfHeader, 0, 0]);

        await Assert.ThrowsAsync<AppException>(() => _temp.Storage.SaveUploadAsync(file, "leaves", OneMb));
    }

    [Fact]
    public async Task SaveUpload_TooLarge_IsRejected()
    {
        var file = TestFiles.Pdf(extraBytes: (int)OneMb);

        await Assert.ThrowsAsync<AppException>(() => _temp.Storage.SaveUploadAsync(file, "leaves", OneMb));
    }

    [Theory]
    [InlineData("../../secret.txt")]
    [InlineData("leaves/../../outside.pdf")]
    public void Open_PathTraversal_IsRejected(string storedPath)
    {
        Assert.Throws<AppException>(() => _temp.Storage.Open(storedPath, "x"));
    }

    [Fact]
    public async Task Open_LegacyUploadsPath_ResolvesInsideRoot()
    {
        var path = await _temp.Storage.SaveUploadAsync(TestFiles.Pdf(), "leaves", OneMb);

        using var file = _temp.Storage.Open("/uploads/" + path, "x")!.Content; // รูปแบบ path เดิมใน DB
        Assert.NotNull(file);
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        var path = await _temp.Storage.SaveUploadAsync(TestFiles.Pdf(), "leaves", OneMb);

        _temp.Storage.Delete(path);

        Assert.Null(_temp.Storage.Open(path, "x"));
    }
}
