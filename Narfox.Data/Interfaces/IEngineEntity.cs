using Narfox.Data.Models;

namespace Narfox.Data.Interfaces;

/// <summary>
/// All game entities that will be managed by the GameStateService should
/// implement this interface. It guarantees that they have the most common
/// properties affected by game engine systems and enables this state
/// management system to merge changes to an entity from engine systems
/// into the data model that is the single source-of-truth for game state
/// </summary>
/// <typeparam name="T">A data model that implements IEntityData</typeparam>
public interface IEngineEntity<T> where T : IEntityData
{
    /// <summary>
    /// The X position of this entity, which may be changed by game engine systems such as collision.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// The Y position of this entity, which may be changed by game engine systems such as collision.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// The rotation of this entity in radians, which may be changed by game engine systems such as physics.
    /// </summary>
    public float RotationRadians { get; set; }

    /// <summary>
    /// The backing data model for this entity, which should act as the single source-of-truth for entity state
    /// </summary>
    public T Model { get; set; }

    /// <summary>
    /// A light struct representing this entity's position last frame, used to merge engine changes to entity
    /// position with non-engine changes in the source-of-truth model.
    /// </summary>
    public GameEntityFrameCache? LastFrame { get; set; }

    /// <summary>
    /// Should apply properties from the Model to the entity
    /// </summary>
    public void UpdateEntityFromModel();
}
