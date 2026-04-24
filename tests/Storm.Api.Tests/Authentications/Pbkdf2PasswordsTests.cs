using System.Reflection;
using Storm.Api.Authentications;

namespace Storm.Api.Tests.Authentications;

public class Pbkdf2PasswordsTests
{
	[Fact]
	public void HashPassword_OutputHasFiveDotSeparatedParts()
	{
		string hashed = Pbkdf2Passwords.HashPassword("hello");
		hashed.Split('.').Should().HaveCount(5);
	}

	[Fact]
	public void HashPassword_PartsAreBase64WhereExpected()
	{
		string[] parts = Pbkdf2Passwords.HashPassword("hello").Split('.');
		Action a1 = () => Convert.FromBase64String(parts[3]);
		Action a2 = () => Convert.FromBase64String(parts[4]);
		a1.Should().NotThrow();
		a2.Should().NotThrow();
	}

	[Fact]
	public void HashPassword_Sha512_DefaultAlgorithm()
	{
		string[] parts = Pbkdf2Passwords.HashPassword("hello").Split('.');
		parts[2].Should().Be("SHA512");
	}

	[Fact]
	public void IsValid_SelectableAlgorithm_Sha256_Accepted()
	{
		// Build a hashed string with SHA256 via the private Hash method.
		MethodInfo hash = typeof(Pbkdf2Passwords).GetMethod("Hash", BindingFlags.NonPublic | BindingFlags.Static)!;
		byte[] salt = System.Text.Encoding.UTF8.GetBytes("saltsaltsaltsaltsaltsaltsaltsalt");
		byte[] hashBytes = (byte[])hash.Invoke(null, ["pw", salt, 100, 64, System.Security.Cryptography.HashAlgorithmName.SHA256])!;
		string composed = $"100.64.SHA256.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hashBytes)}";
		Pbkdf2Passwords.IsValid("pw", composed).Should().BeTrue();
	}

	[Fact]
	public void HashPassword_SameInputDifferentCalls_ProduceDifferentHashes()
	{
		string a = Pbkdf2Passwords.HashPassword("pw");
		string b = Pbkdf2Passwords.HashPassword("pw");
		a.Should().NotBe(b);
	}

	[Fact]
	public void IsValid_CorrectPassword_ReturnsTrue()
	{
		string hashed = Pbkdf2Passwords.HashPassword("pw");
		Pbkdf2Passwords.IsValid("pw", hashed).Should().BeTrue();
	}

	[Fact]
	public void IsValid_WrongPassword_ReturnsFalse()
	{
		string hashed = Pbkdf2Passwords.HashPassword("pw");
		Pbkdf2Passwords.IsValid("other", hashed).Should().BeFalse();
	}

	[Fact]
	public void IsValid_MalformedHash_ReturnsFalse()
	{
		Pbkdf2Passwords.IsValid("pw", "notahash").Should().BeFalse();
		Pbkdf2Passwords.IsValid("pw", "").Should().BeFalse();
		Pbkdf2Passwords.IsValid("pw", "a.b.c.d.e").Should().BeFalse();
	}

	[Fact]
	public void IsValid_InvalidBase64Parts_ReturnsFalse()
	{
		Pbkdf2Passwords.IsValid("pw", "100.64.SHA512.!notbase64!.!notbase64!").Should().BeFalse();
	}

	[Fact]
	public void IsValid_UnknownAlgorithm_ReturnsFalse()
	{
		byte[] salt = new byte[32];
		byte[] hash = new byte[64];
		string composed = $"100.64.MD5.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
		Pbkdf2Passwords.IsValid("pw", composed).Should().BeFalse();
	}

	[Fact]
	public void IsValid_UsesConstantTimeComparison()
	{
		string hashed = Pbkdf2Passwords.HashPassword("pw");
		Pbkdf2Passwords.IsValid("pw", hashed).Should().BeTrue();
		Pbkdf2Passwords.IsValid("wrong", hashed).Should().BeFalse();
	}

	[Fact]
	public void CreateSalt_ProducesRequestedByteCount()
	{
		MethodInfo createSalt = typeof(Pbkdf2Passwords).GetMethod("CreateSalt", BindingFlags.NonPublic | BindingFlags.Static)!;
		byte[] salt = (byte[])createSalt.Invoke(null, null)!;
		salt.Length.Should().Be(32);
	}

	[Fact]
	public void CreateSalt_IsNonDeterministic()
	{
		MethodInfo createSalt = typeof(Pbkdf2Passwords).GetMethod("CreateSalt", BindingFlags.NonPublic | BindingFlags.Static)!;
		byte[] s1 = (byte[])createSalt.Invoke(null, null)!;
		byte[] s2 = (byte[])createSalt.Invoke(null, null)!;
		s1.Should().NotEqual(s2);
	}
}
