// ReSharper disable once CheckNamespace

namespace MidjourneyAPI.Core;

public class MjAccount
{
    /// <summary>
    /// 用户ID。
    /// </summary>
    public Guid UserId { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? WebsocketToken { get; set; }

    public required string UserToken { get; set; }

    public string? UserJson { get; set; }
}