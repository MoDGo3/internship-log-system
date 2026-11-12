using System.Security.Cryptography;
using System.Text;

namespace InternLog.API.Core.Security;

/// <summary>
/// Fast SHA-256 hasher for non-password secrets (refresh tokens, password-reset tokens).
/// Do NOT use for passwords — use PasswordHasher (BCrypt) for those.
/// Fixes DoS vulnerability: instead of BCrypt-iterating all tokens, we hash the incoming
/// token once (O(1)) and do a direct indexed DB lookup.
/// </summary>
public static class Sha256Hasher
{
    /// <summary>
    /// Returns the lowercase hex SHA-256 digest of <paramref name="token"/>.
    /// </summary>
    public static string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        var bytes  = Encoding.UTF8.GetBytes(token);
        var digest = SHA256.HashData(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}
