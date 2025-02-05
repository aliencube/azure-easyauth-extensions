using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aliencube.Azure.Extensions.EasyAuth;

/// <summary>
/// This represents the entity for the MS Client Principal from the request header of "X-MS-CLIENT-PRINCIPAL".
/// </summary>
public class MsClientPrincipal
{
    private static readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Gets or sets the identity provider ("auth_type").
    /// </summary>
    [JsonPropertyName("auth_typ")]
    public string? IdentityProvider { get; set; }

    /// <summary>
    /// Gets or sets the name claim type ("name_typ").
    [JsonPropertyName("name_typ")]
    public string? NameClaimType { get; set; }

    /// <summary>
    /// Gets or sets the role claim type ("role_typ").
    /// </summary>
    [JsonPropertyName("role_typ")]
    public string? RoleClaimType { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="MsClientPrincipalClaim"/> objects ("claims").
    /// </summary>
    [JsonPropertyName("claims")]
    public IEnumerable<MsClientPrincipalClaim>? Claims { get; set; }

    /// <summary>
    /// Parses the client principal header value to <see cref="MsClientPrincipal"/> instance.
    /// </summary>
    /// <param name="value">Client Principal header value.</param>
    /// <returns>Returns <see cref="MsClientPrincipal"/> instance.</returns>
    public static async Task<MsClientPrincipal?> ParseMsClientPrincipal(string value)
    {
        var decoded = Convert.FromBase64String(value);
        using var stream = new MemoryStream(decoded);
        var principal = await JsonSerializer.DeserializeAsync<MsClientPrincipal>(stream, options).ConfigureAwait(false);

        return principal;
    }
}
