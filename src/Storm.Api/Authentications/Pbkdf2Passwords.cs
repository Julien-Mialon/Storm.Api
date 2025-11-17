using System.Security.Cryptography;
using Storm.Api.Extensions;

namespace Storm.Api.Authentications;

public static class Pbkdf2Passwords
{
	private const int SALT_SIZE = 32;
	private const int KEY_SIZE = 64;
	private const int ITERATIONS = 300_000;
	private static readonly HashAlgorithmName HASH_ALGORITHM = HashAlgorithmName.SHA512;


	public static bool IsValid(string inputPassword, string password)
	{
		if (password.IsNullOrEmpty())
		{
			return false;
		}

		string[] passwordParts = password.Split('.');
		if (passwordParts.Length != 5)
		{
			return false; // Invalid format
		}

		if (int.TryParse(passwordParts[0], out int iterations) is false ||
		    int.TryParse(passwordParts[1], out int keySize) is false ||
		    passwordParts[2] is not "SHA256" and not "SHA512" ||
		    TryConvertFromBase64(passwordParts[3], out byte[] salt) is false ||
		    TryConvertFromBase64(passwordParts[4], out byte[] hash) is false)
		{
			return false; // Invalid format
		}

		HashAlgorithmName hashAlgorithm = passwordParts[2] switch
		{
			"SHA256" => HashAlgorithmName.SHA256,
			"SHA512" => HashAlgorithmName.SHA512,
			_ => throw new InvalidOperationException($"Unsupported hash algorithm {passwordParts[2]}")
		};

		byte[] inputHash = Hash(inputPassword, salt, iterations, keySize, hashAlgorithm);
		return CryptographicOperations.FixedTimeEquals(inputHash, hash);
	}

	public static string HashPassword(string password)
	{
		byte[] salt = CreateSalt();
		byte[] hash = Hash(password, salt, ITERATIONS, KEY_SIZE, HASH_ALGORITHM);

		return $"{ITERATIONS}.{KEY_SIZE}.{HASH_ALGORITHM}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
	}

	private static byte[] Hash(string password, byte[] salt, int iterations, int keySize, HashAlgorithmName hashAlgorithm)
	{
		return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
	}

	private static byte[] CreateSalt()
	{
		return RandomNumberGenerator.GetBytes(SALT_SIZE);
	}

	private static bool TryConvertFromBase64(string base64String, out byte[] result)
	{
		try
		{
			result = Convert.FromBase64String(base64String);
			return true;
		}
		catch
		{
			result = [];
			return false;
		}
	}
}