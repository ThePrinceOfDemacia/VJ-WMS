using VjWms.Shared.Enums;

namespace VjWms.Shared.DTOs;

public class SyncRequestDto
{
    public string ClientId { get; set; } = string.Empty;
    public DateTimeOffset LastSyncAt { get; set; }
    public List<SyncEntityDto> Entities { get; set; } = new List<SyncEntityDto>();
}

public class SyncResponseDto
{
    public DateTimeOffset ServerTimestamp { get; set; }
    public List<SyncEntityDto> AcceptedEntities { get; set; } = new List<SyncEntityDto>();
    public List<SyncConflictDto> RejectedEntities { get; set; } = new List<SyncConflictDto>();
    public List<SyncEntityDto> ServerUpdates { get; set; } = new List<SyncEntityDto>();
}

public class SyncEntityDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class SyncConflictDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserProfileDto User { get; set; } = new UserProfileDto();
}

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string[] Permissions { get; set; } = Array.Empty<string>();
}
