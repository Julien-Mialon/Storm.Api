namespace Storm.Api.Authentications.Jwts;

/// <summary>
/// Marker type used solely as a generic parameter to distinguish
/// JwtConfiguration&lt;RefreshTokenMarker&gt; / JwtService&lt;RefreshTokenMarker&gt;
/// from the access-token equivalents in the DI container.
/// </summary>
public sealed class RefreshTokenMarker;
