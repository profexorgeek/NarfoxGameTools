using Narfox.Data.Interfaces;
using Narfox.Data.Models;

namespace Narfox.Data.Extensions;

public static class EntityExtensions
{
    public static void Synchronize<T>(this IEngineEntity<T> entity, GameStateService svc) where T : IEntityData
    {
        // make sure we have a valid last frame or we can't calculate a delta
        if(entity.LastFrame != null)
        {
            // find the delta between the current frame and the last frame
            var delta = new GameEntityFrameCache(
                entity.X - entity.LastFrame.Value.X,
                entity.Y - entity.LastFrame.Value.Y,
                entity.RotationRadians - entity.LastFrame.Value.RotationRadians);

            // request that the state service applies the delta to the current version of the model
            // which may not match our current entity if it was changed by some other source
            svc.RequestApplyEngineDelta(entity.Model.Id, svc.LocalClient, delta);

            // the game state service should now have the latest model with engine changes merged in
            // now we need to apply the model
            entity.UpdateEntityFromModel();
        }

        // finally, update our LastFrame since we should be at the end of this frame
        entity.LastFrame = new GameEntityFrameCache(entity.X, entity.Y, entity.RotationRadians);

    }

}
