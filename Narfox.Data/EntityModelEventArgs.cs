using Narfox.Data.Interfaces;

namespace Narfox.Data;

public class EntityModelEventArgs : EventArgs
{
    public IEntityData AffectedModel { get; set; }

    public EntityModelEventArgs(IEntityData model)
    {
        AffectedModel = model;
    }
}
