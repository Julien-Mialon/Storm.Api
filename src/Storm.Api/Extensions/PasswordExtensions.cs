using System.Security.Cryptography;
using System.Text;

namespace Storm.Api.Extensions;

public static class PasswordExtensions
{
	public static string AsSha256(this string input)
	{
		using SHA256 algorithm = SHA256.Create();
		byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
		return hash.ToHexString();
	}
}