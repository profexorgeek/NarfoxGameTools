namespace Narfox.Data.Interfaces;

/// <summary>
/// This interface should be implemented by any data model that
/// will act as the source-of-truth for a game entity. These properties
/// ensure that this model can be tracked throughout its lifespan
/// and merge data from multiple sources into a single model.
/// </summary>
public interface IEntityData
{
    /// <summary>
    /// The unique ID of the entity across all clients
    /// </summary>
    public ushort Id { get; set; }

    /// <summary>
    /// The unique ID of the client that owns this entity
    /// </summary>
    public ushort OwnerId { get; set; }

    /// <summary>
    /// The type name this entity is associated with, helps a
    /// game engine know what type of game entity to create for
    /// binding to this model
    /// </summary>
    public string EntityTypeName { get; set; }
}