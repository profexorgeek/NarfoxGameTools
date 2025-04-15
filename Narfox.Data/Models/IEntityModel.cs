namespace Narfox.Data.Models;

public interface IEntityModel
{
    public ushort Id { get; set; }

    public ushort OwnerId { get; set; }

    public string EntityTypeName { get; set; }
}
