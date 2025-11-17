namespace Storm.Api.Vaults;

public class VaultConfiguration
{
	public required string Address { get; set; }
	public required string Token { get; set; }
	public required string MountPoint { get; set; }
	public required string[] Keys { get; set; }
}