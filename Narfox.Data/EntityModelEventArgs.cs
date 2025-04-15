using Narfox.Data.Models;

namespace Narfox.Data;

public class EntityModelEventArgs : EventArgs
{
    public IEntityModel AffectedModel { get; set; }

    public EntityModelEventArgs(IEntityModel model)
    {
        AffectedModel = model;
    }
}
