using System.Text;

namespace Storm.Api.Extensions;

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
}