using Narfox.Data.Enums;
using Narfox.Data.Models;
using Narfox.Extensions;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;

namespace Narfox.Data
{
    /// <summary>
    /// Centralizes an authoritative flat collection of all game objects. Provides
    /// a method to request changes to a model and can register specific property
    /// types to interpolate instead of directly changed.
    /// 
    /// The intended pattern is that the data model is the master state-holder for
    /// all game entities owned by the engine. When the engine wants to change something
    /// such as position, it should make a state change request and this state service
    /// will manage updating the state. Then, the game engine entity should update itself
    /// from the authoritative model at the end of every frame.
    /// 
    /// ModelAdded and ModelDestroyed events allow listening for critical events.
    /// 
    /// The Update method should be called by the game engine every frame.
    /// </summary>
    public class GameStateService
    {
        /// <summary>
        /// This class is used internally to package up model
        /// change requests with metadata for a queue
        /// </summary>
        private class ModelChangeRequest
        {
            public ushort Id { get; set; }
            public ActionType Action { get; set; }
            public Client Requestor { get; set; }
            public Dictionary<string, object> ChangedProperties { get; set; }

            public ModelChangeRequest(ushort id, ActionType action, Client requestor, Dictionary<string, object> changes)
            {
                Id = id;
                Action = action;
                Requestor = requestor;
                ChangedProperties = changes;
            }
        }


        List<IEntityModel> trackedModels;


        /// <summary>
        /// Called when a new model is added to the service. The implementing
        /// game should bind this to create a game entity that tracks the model
        /// </summary>
        public event EventHandler<EntityModelEventArgs>? ModelAdded;

        /// <summary>
        /// Called when a model is destroyed. The implementing game should destroy
        /// any game objects associated with this model
        /// </summary>
        public event EventHandler<EntityModelEventArgs>? ModelDestroyed;



        /// <summary>
        /// The client that is running this application instance.
        /// </summary>
        public Client LocalClient { get; set; }

        /// <summary>
        /// The client that is the authority over models. By default, this is
        /// just the local client but could be some third party concept of
        /// a client such as a network client.
        /// 
        /// Used to make sure that the client requesting model changes 
        /// has authority to do so.
        /// </summary>
        public Client Authority { get; private set; }



        /// <summary>
        /// Returns a new instance of the GameStateService
        /// </summary>
        /// <param name="localClient">An optional local client. A default one will be created if not provided.</param>
        public GameStateService(Client localClient = null)
        {
            trackedModels = new List<IEntityModel>();

            if (localClient == null)
            {
                localClient = new Client
                {
                    Id = 0,
                    Name = "CLIENT",
                };
            }

            // by default, the local client is the authority over all data
            // this is only different in a networked environment
            LocalClient = localClient;
            Authority = localClient;
        }

        /// <summary>
        /// Registers a new client authority. The authority client has the right
        /// to override models with reckoning messages.
        /// </summary>
        /// <param name="newAuthority">The new authority client</param>
        public void RegisterAuthority(Client newAuthority)
        {
            Authority = newAuthority;
        }

        /// <summary>
        /// Requests changes to an existing model.
        /// </summary>
        /// <param name="id">The unique ID of the model to be changed</param>
        /// <param name="requestor">The change requestor</param>
        /// <param name="changes">A key value list of changes, the value type must match the target model.</param>
        public void RequestUpdateModel(ushort id, Client requestor, Dictionary<string, object> changes)
        {
            var model = trackedModels.FirstOrDefault(m => m.Id == id);
            
            // EARLY OUT: can't change unknown model
            if(model == null)
            {
                return;
            }


            // reflect properties
            var properties = model.GetType().GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance);

            // set all properties on the existing model to match the target model, using reflection
            // ensures that we don't break references to our list of registered entities
            foreach (var prop in properties)
            {
                // ignore properties we can't or shouldn't change
                if (prop.CanRead == false ||
                    prop.CanWrite == false ||
                    prop.Name == nameof(IEntityModel.Id) ||
                    prop.Name == nameof(IEntityModel.OwnerId) ||
                    prop.Name == nameof(IEntityModel.EntityTypeName))
                    continue;

                foreach (var key in changes.Keys)
                {
                    if (prop.Name == key)
                    {
                        prop.SetValue(model, changes[key]);
                    }
                }
            }
        }

        /// <summary>
        /// Requests a model to be created
        /// </summary>
        /// <param name="model">The model to add to the tracked models</param>
        /// <param name="requestor">The requestor</param>
        public void RequestCreateModel(IEntityModel model, Client requestor)
        {
            var exists = trackedModels.Any(t => t.Id == model.Id);
            if(exists == false && model.OwnerId == requestor.Id)
            {
                trackedModels.Add(model);
                ModelAdded?.Invoke(requestor, new EntityModelEventArgs(model));
            }
        }

        /// <summary>
        /// Requests that a model be deleted.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestor"></param>
        public void RequestDeleteModel(ushort id, Client requestor)
        {
            var existing = trackedModels.FirstOrDefault(t => t.Id == id);
            if(existing != null && existing.OwnerId == requestor.Id)
            {
                trackedModels.Remove(existing);
                var args = new EntityModelEventArgs(existing);
                ModelDestroyed?.Invoke(requestor, args);
            }
        }

        /// <summary>
        /// Forces an existing model to match an incoming model. Will not change
        /// the existing model's reference, it will shallow copy all properties
        /// except the core identifying properties defined in the IEntityModel
        /// </summary>
        /// <param name="id">The ID of the target model</param>
        /// <param name="requestor">The reckoning requestor</param>
        /// <param name="incomingModel">The model to match</param>
        public void RequestReckonModel(ushort id, Client requestor, IEntityModel incomingModel)
        {
            var existing = trackedModels.FirstOrDefault(t => t.Id == id);
            if(existing != null && requestor.Id == Authority.Id && incomingModel.GetType() == existing.GetType())
            {
                // reflect properties
                var properties = existing.GetType().GetProperties(
                    BindingFlags.Public |
                    BindingFlags.Instance);

                // set all properties on the existing model to match the target model, using reflection
                // ensures that we don't break references to our list of registered entities
                foreach (var prop in properties)
                {
                    // ignore properties we can't or shouldn't change
                    if (prop.CanRead == false ||
                        prop.CanWrite == false ||
                        prop.Name == nameof(IEntityModel.Id) ||
                        prop.Name == nameof(IEntityModel.OwnerId) ||
                        prop.Name == nameof(IEntityModel.EntityTypeName))
                        continue;

                    var newValue = prop.GetValue(incomingModel);
                    prop.SetValue(existing, newValue);
                }
            }
        }


        /// <summary>
        /// Should be called in the main engine loop, performs any state service updates
        /// </summary>
        public virtual void Update()
        {
            
        }
    }
}
