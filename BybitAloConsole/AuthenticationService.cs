using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BybitAloConsole
{
    public static class AuthenticationService
    {
        static readonly DateTime Jan1St1970 = new DateTime
            (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1St1970).TotalMilliseconds;
        }

        public static string CreateSignature(string secret, string message)
        {
            var signatureBytes = HmacSha256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));
            return ByteArrayToString(signatureBytes);
        }

        static byte[] HmacSha256(byte[] keyByte, byte[] messageBytes)
        {
            using var hash = new HMACSHA256(keyByte);
            return hash.ComputeHash(messageBytes);
        }

        static string ByteArrayToString(IReadOnlyCollection<byte> byteArray)
        {
            var hex = new StringBuilder(byteArray.Count * 2);

            foreach (var b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}