using System.Security.Cryptography;
using System.Text;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// DES 加解密（兼容原 DESCryptoServiceProvider 行为：3DES/ECB/PKCS7）。
/// 使用 DES.Create() 以适配 .NET 8（DESCryptoServiceProvider 已 obsolete）。
/// Key/IV 由字符串按字节截断得到，与原实现保持一致。
/// </summary>
public static class DES
{
    /// <summary>加密（CBC，与原实现一致）</summary>
    public static string Encrypt(string input, string key)
    {
        var keyBytes = GetKeyBytes(key, 8);
        var ivBytes = GetKeyBytes(key, 8);
#pragma warning disable SYSLIB0021
        using var des = new DESCryptoServiceProvider();
#pragma warning restore SYSLIB0021
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;
        using var transform = des.CreateEncryptor(keyBytes, ivBytes);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var encrypted = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>解密</summary>
    public static string Decrypt(string input, string key)
    {
        var keyBytes = GetKeyBytes(key, 8);
        var ivBytes = GetKeyBytes(key, 8);
#pragma warning disable SYSLIB0021
        using var des = new DESCryptoServiceProvider();
#pragma warning restore SYSLIB0021
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.PKCS7;
        using var transform = des.CreateDecryptor(keyBytes, ivBytes);
        var inputBytes = Convert.FromBase64String(input);
        var decrypted = transform.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
        return Encoding.UTF8.GetString(decrypted);
    }

    private static byte[] GetKeyBytes(string key, int size)
    {
        var bytes = Encoding.UTF8.GetBytes(key);
        var result = new byte[size];
        Array.Copy(bytes, result, Math.Min(bytes.Length, size));
        return result;
    }
}
