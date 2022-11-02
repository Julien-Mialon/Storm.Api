namespace Storm.Api.Vaults;

public class VaultConfiguration
{
	public string Address { get; set; }
	public string Token { get; set; }
	public string MountPoint { get; set; }
	public string[] Keys { get; set; }
}