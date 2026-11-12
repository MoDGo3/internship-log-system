using Microsoft.AspNetCore.StaticFiles;
using InternLog.API.Core.Common;

namespace InternLog.API.Core.Storage;

/// <summary>ไฟล์ที่เปิดอ่านแล้ว พร้อมส่งกลับเป็น response</summary>
public sealed record StoredFile(Stream Content, string ContentType, string DownloadName);

/// <summary>
/// ที่เก็บไฟล์อัปโหลด — เก็บนอก wwwroot เพื่อไม่ให้เข้าถึงได้โดยตรงจากเว็บ
/// ทุกการดาวน์โหลดต้องผ่าน endpoint ที่ตรวจสิทธิ์ก่อนเรียก <see cref="Open"/>
/// </summary>
public interface IFileStorage
{
    /// <summary>ตรวจขนาด นามสกุล และ magic bytes แล้วบันทึกไฟล์ คืน path ที่ใช้เก็บใน DB</summary>
    Task<string> SaveUploadAsync(IFormFile file, string category, long maxBytes);

    /// <summary>บันทึกไฟล์ที่ระบบสร้างเอง (เช่น PDF) คืน path ที่ใช้เก็บใน DB</summary>
    Task<string> SaveGeneratedAsync(byte[] content, string category, string extension);

    /// <summary>เปิดไฟล์จาก path ใน DB — คืน null ถ้าไม่พบไฟล์</summary>
    StoredFile? Open(string? storedPath, string downloadName);

    /// <summary>ลบไฟล์ (ไม่ error ถ้าไม่มีไฟล์)</summary>
    void Delete(string? storedPath);
}

public class LocalFileStorage : IFileStorage
{
    // magic bytes ของแต่ละนามสกุลที่อนุญาต
    private static readonly Dictionary<string, byte[]> Signatures = new()
    {
        [".pdf"]  = [0x25, 0x50, 0x44, 0x46],
        [".jpg"]  = [0xFF, 0xD8, 0xFF],
        [".jpeg"] = [0xFF, 0xD8, 0xFF],
        [".png"]  = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A],
        [".doc"]  = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1],
        [".docx"] = [0x50, 0x4B, 0x03, 0x04],
    };

    // path แบบเก่าที่เคยเก็บใน DB ตอนไฟล์อยู่ใน wwwroot
    private const string LegacyPrefix = "/uploads/";

    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    private readonly string _root;

    public LocalFileStorage(IConfiguration config, IWebHostEnvironment env)
    {
        var configured = config["FileStorage:Root"];
        _root = Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "uploads")
            : configured);
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveUploadAsync(IFormFile file, string category, long maxBytes)
    {
        if (file.Length == 0)
            throw AppException.BadRequest("ไฟล์ว่างเปล่า");

        if (file.Length > maxBytes)
            throw AppException.BadRequest($"ขนาดไฟล์ต้องไม่เกิน {maxBytes / (1024 * 1024)} MB");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Signatures.TryGetValue(extension, out var signature))
            throw AppException.BadRequest("รองรับเฉพาะไฟล์ .pdf, .jpg, .jpeg, .png, .doc, .docx เท่านั้น");

        if (!await HasSignatureAsync(file, signature))
            throw AppException.BadRequest("เนื้อหาไฟล์ไม่ตรงกับนามสกุล");

        var storedPath = NewStoredPath(category, extension);
        await using var output = new FileStream(ToFullPath(storedPath), FileMode.CreateNew);
        await file.CopyToAsync(output);
        return storedPath;
    }

    public async Task<string> SaveGeneratedAsync(byte[] content, string category, string extension)
    {
        var storedPath = NewStoredPath(category, extension);
        await File.WriteAllBytesAsync(ToFullPath(storedPath), content);
        return storedPath;
    }

    public StoredFile? Open(string? storedPath, string downloadName)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return null;

        var fullPath = ToFullPath(storedPath);
        if (!File.Exists(fullPath)) return null;

        if (!ContentTypes.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        return new StoredFile(File.OpenRead(fullPath), contentType, downloadName);
    }

    public void Delete(string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath)) return;

        var fullPath = ToFullPath(storedPath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private string NewStoredPath(string category, string extension)
    {
        Directory.CreateDirectory(Path.Combine(_root, category));
        // ไม่ใช้ชื่อไฟล์จากผู้ใช้บน disk — ชื่อเดิมเก็บใน DB แยกต่างหาก
        return $"{category}/{Guid.NewGuid():N}{extension}";
    }

    /// <summary>แปลง path ใน DB เป็น path จริง และกันไม่ให้หลุดออกนอก root (path traversal)</summary>
    private string ToFullPath(string storedPath)
    {
        var relative = storedPath.StartsWith(LegacyPrefix, StringComparison.Ordinal)
            ? storedPath[LegacyPrefix.Length..]
            : storedPath.TrimStart('/');

        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw AppException.BadRequest("path ของไฟล์ไม่ถูกต้อง");

        return fullPath;
    }

    private static async Task<bool> HasSignatureAsync(IFormFile file, byte[] signature)
    {
        if (file.Length < signature.Length) return false;

        var header = new byte[signature.Length];
        await using var stream = file.OpenReadStream();
        await stream.ReadExactlyAsync(header);
        return header.AsSpan().SequenceEqual(signature);
    }
}
