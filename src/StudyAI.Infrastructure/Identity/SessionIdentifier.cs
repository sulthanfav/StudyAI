using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace StudyAI.Infrastructure.Identity;

internal static class SessionIdentifier
{
    public const string ClaimType = "studyai:session_id";

    public static string Create() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public static byte[] Hash(string identifier) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(identifier));
}
