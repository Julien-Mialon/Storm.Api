using System.Security.Cryptography;
using System.Text;

namespace Storm.Api.Core.Extensions;

public static class ByteExtensions
{
	public static string ToHexString(this byte[] input)
	{
		StringBuilder formatted = new(2 * input.Length);
		foreach (byte b in input)
		{
			formatted.AppendFormat("{0:X2}", b);
		}

		return formatted.ToString();
	}

	public static string AsSha256(this string input)
	{
		using SHA256 algorithm = SHA256.Create();
		byte[] hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
		return hash.ToHexString();
	}
}