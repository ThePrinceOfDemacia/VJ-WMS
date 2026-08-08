namespace VjWms.Shared.Interfaces;

/// <summary>
/// Base interface for entities that participate in sync
/// </summary>
public interface ISyncable
{
    string Id { get; set; }
    string SyncStatus { get; set; }
    string CreatedAt { get; set; }
    string UpdatedAt { get; set; }
    int Version { get; set; }
}

/// <summary>
/// Base interface for all entities with audit fields
/// </summary>
public interface IAuditable
{
    string CreatedBy { get; set; }
    string CreatedAt { get; set; }
    string UpdatedAt { get; set; }
}
