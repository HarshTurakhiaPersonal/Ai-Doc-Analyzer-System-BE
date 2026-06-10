using System.Security.Cryptography;
using System.Text;

namespace Application.Common;

public static class SecurityHelper
{
    public static string ComputeSha256Hash(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));

        return Convert.ToHexString(bytes);
    }
}