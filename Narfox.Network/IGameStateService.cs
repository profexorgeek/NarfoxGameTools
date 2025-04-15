namespace Narfox.Data;

public interface IDataDrivenGameObject
{
    public object Model { get; set; }

    public object GetModel();

    public void SetModel(object model);
}

public interface IGameStateService
{


    public void RegisterGameObject(IDataDrivenGameObject gameObject);

    public void Update();

}
