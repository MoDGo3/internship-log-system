namespace InternLog.API.Core.Security;

/// <summary>
/// BCrypt wrapper สำหรับ hash/verify password
/// ใช้ work factor 12 (OWASP recommendation)
/// </summary>
public static class PasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>Hash รหัสผ่านด้วย BCrypt</summary>
    public static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    /// <summary>ตรวจสอบ password กับ hash ที่เก็บในฐานข้อมูล</summary>
    public static bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
