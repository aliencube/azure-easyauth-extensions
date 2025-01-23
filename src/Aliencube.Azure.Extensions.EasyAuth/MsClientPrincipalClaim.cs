using System.Text.Json.Serialization;

namespace Aliencube.Azure.Extensions.EasyAuth;

/// <summary>
/// This represents the entity for the MS Client Principal claim.
/// </summary>
public class MsClientPrincipalClaim
{
    /// <summary>
    /// Gets or sets the claim type ("typ").
    /// </summary>
    [JsonPropertyName("typ")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the claim value ("val").
    /// </summary>
    [JsonPropertyName("val")]
    public string? Value { get; set; }
}
