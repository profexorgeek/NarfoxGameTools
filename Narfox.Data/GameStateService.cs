using Narfox.Data.Enums;
using Narfox.Data.Models;
using Narfox.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Reflection;
using static System.Collections.Specialized.BitVector32;

namespace Narfox.Data
{
    public class GameStateService
    {
        private class QueueModelAction
        {
            public ActionType Action { get; set; }
            public IEntityModel Model { get; set; }
            public Client Requestor { get; set; }
            

            public QueueModelAction(IEntityModel model, ActionType action, Client requestor)
            {
                Model = model;
                Action = action;
                Requestor = requestor;
            }
        }


        List<IEntityModel> trackedModels;
        List<QueueModelAction> modelChangeQueue;
        Dictionary<string, float> lerpProperties;



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
        /// How close two numeric values must be to be considered "the same". Helps
        /// prevent forever lerping by amounts that may be smaller than float precision.
        /// </summary>
        public float LerpCompleteThreshold { get; set; } = 0.1f;

        /// <summary>
        /// When lerping properties, how much to lerp each update cycle.
        /// </summary>
        public float LerpAmountPerFrame { get; set; } = 0.1f;




        /// <summary>
        /// Returns a new instance of the GameStateService
        /// </summary>
        /// <param name="localClient">An optional local client. A default one will be created if not provided.</param>
        public GameStateService(Client localClient = null)
        {
            trackedModels = new List<IEntityModel>();
            modelChangeQueue = new List<QueueModelAction>();
            lerpProperties = new Dictionary<string, float>();

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
        /// Registers a property name on models that should always lerp over several frames
        /// when merging models. NOTE: only numeric properties can lerp!
        /// </summary>
        /// <param name="propertyName">The property name to lerp</param>
        /// <param name="lerpPerFrame">The amount of lerp to apply each frame</param>
        public void RegisterLerpProperty(string propertyName, float lerpPerFrame = 0.2f)
        {
            if(lerpProperties.ContainsKey(propertyName))
            {
                lerpProperties[propertyName] = lerpPerFrame;
            }
            else
            {
                lerpProperties.Add(propertyName, lerpPerFrame);
            }
        }

        /// <summary>
        /// Registers multiple lerp properties at once.
        /// </summary>
        /// <param name="lerps">A dictionary of property names and lerp amount per frame</param>
        public void RegisterLerpProperty(Dictionary<string, float> lerps)
        {
            foreach(var key in lerps.Keys)
            {
                RegisterLerpProperty(key, lerps[key]);
            }
        }

        /// <summary>
        /// Enqueues a request to perform some action on a provided model and tracks
        /// the requestor. Models added to a queue will be merged with existing models
        /// with any lerpable properties being lerped over time and removed from the
        /// queue once changes are fully applied.
        /// </summary>
        /// <param name="updatedModel">An IEntityModel representing the new state</param>
        /// <param name="action">The type of action, such as create, update, or delete</param>
        /// <param name="requestor">The requesting client, whose authority over the model will be checked</param>
        public void TryEnqueueModelChange(IEntityModel updatedModel, ActionType action, Client requestor)
        {
            // EARLY OUT: for a request to be honored, either the message must be a reckoning issued by the
            // authority OR the requestor must be the owner of the updated model
            if(updatedModel.OwnerId != requestor.Id &&
                requestor.Id != Authority.Id)
            {
                return;
            }

            // add or update the model to the queue
            var alreadyEnqueued = modelChangeQueue.Where(q => q.Model.Id == updatedModel.Id &&
            q.Action == action).FirstOrDefault();
            if(alreadyEnqueued == null)
            {
                modelChangeQueue.Add(new QueueModelAction(updatedModel, action, requestor));
            }
            else
            {
                alreadyEnqueued.Model = updatedModel;
            }
        }

        /// <summary>
        /// Processes model changes waiting in the change queue. Should be called in a main
        /// tick or engine loop
        /// </summary>
        public virtual void Update()
        {
            for (var i = modelChangeQueue.Count - 1; i > -1; i--)
            {
                var q = modelChangeQueue[i];
                bool processingComplete = false;
                switch (q.Action)
                {
                    case ActionType.None:
                        // NOOP
                        break;
                    case ActionType.Create:
                        processingComplete = CreateQueuedItem(q);
                        break;
                    case ActionType.Update:
                        processingComplete = UpdateQueuedItem(q);
                        break;
                    case ActionType.Destroy:
                        processingComplete = DestroyQueuedItem(q);
                        break;
                    case ActionType.Reckon:
                        processingComplete = ReckonQueuedItem(q);
                        break;
                }

                // if this item is fully processed, we remove it from the queue
                // some items may lerp over many frames
                if(processingComplete)
                {
                    modelChangeQueue.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// If the queued model doesn't already exist in our tracked models, we add
        /// it and raise the ModelAdded event for any subscribers.
        /// </summary>
        /// <param name="q">A queue item</param>
        /// <returns>True if this item has been fully processed and can be removed from the queue</returns>
        bool CreateQueuedItem(QueueModelAction q)
        {
            var existing = trackedModels.Where(t => t.Id == q.Model.Id).FirstOrDefault();
            if (existing == null)
            {
                trackedModels.Add(q.Model);
                ModelAdded?.Invoke(q.Requestor, new EntityModelEventArgs(q.Model));
            }
            else
            {
                // NOOP: likely this is a stale request
            }

            // there's no lerping or stalling here so we always return true
            // because this should be fully processed
            return true;
        }

        /// <summary>
        /// Updates an existing model by looping through each of its properties and setting the
        /// existing model's properties to matching properties from the queued model.
        /// 
        /// If the property name exists in the list of registered properties to interpolate, it
        /// will interpolate the values instead of directly setting them. The queued item will
        /// stay in the queue until the lerping process has reached the threshold defined by
        /// the LerpCompleteThreshold
        /// </summary>
        /// <param name="q">A queued model to update</param>
        /// <returns></returns>
        bool UpdateQueuedItem(QueueModelAction q)
        {
            var existing = trackedModels.Where(t => t.Id == q.Model.Id).FirstOrDefault();

            // EARLY OUT: no model found so processing is not complete
            if(existing == null)
            {
                return false;
            }

            // EARLY OUT: models are the same reference or the new one is null
            if(q.Model == null || q.Model == existing)
            {
                Debug.Assert(false, "Models were null or the same reference!");
                return true;
            }

            // reflect properties
            var properties = existing.GetType().GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance);

            bool isComplete = true;

            // set all properties on the existing model to match the target model, using reflection
            // ensures that we don't break references to our list of registered entities
            foreach (var prop in properties)
            {
                // ignore properties we can't change
                if (!prop.CanRead || prop.CanWrite)
                    continue;

                var existingValue = prop.GetValue(existing);
                var targetValue = prop.GetValue(q.Model);

                // we can lerp if both values are valid and the property type is numeric
                if(lerpProperties.ContainsKey(prop.Name) &&
                    existingValue != null &&
                    targetValue != null &&
                    MathExtensions.IsNumericType(prop.PropertyType))
                {
                    var lerpedValue = MathExtensions.LerpNumeric(existingValue, targetValue, LerpAmountPerFrame);
                    prop.SetValue(existing, lerpedValue);

                    // if our new value is still too far away from the target value, we mark this
                    // as incomplete so the model stays in queue
                    double delta = MathExtensions.GetDelta(lerpedValue, targetValue);
                    if(delta > LerpCompleteThreshold)
                    {
                        isComplete = false;
                    }
                }

                // otherwise we just snap to the new value
                else
                {
                    prop.SetValue(existing, targetValue);
                }
            }

            return isComplete;
        }

        bool DestroyQueuedItem(QueueModelAction q)
        {
            // TODO: remove any other outstanding requests in case so we don't try to update models
            // that no longer exist forever
        }

        bool ReckonQueuedItem(QueueModelAction q)
        {

        }


        protected virtual void CreateModel(IEntityModel model, Client requestor)
        {
            var existing = trackedModels.Where(t => t.Id == model.Id).FirstOrDefault();
            if (existing != null)
            {
                MergeModels(existing, model);
            }
            else
            {
                trackedModels.Add(model);
                ModelAdded?.Invoke(requestor, new EntityModelEventArgs(model));
            }
        }

        protected virtual void UpdateModel(IEntityModel model, Client requestor)
        {

        }

        public virtual void MergeModels(IEntityModel existingModel, IEntityModel newModel)
        {
            if (existingModel == null || newModel == null || existingModel == newModel)
                throw new ArgumentException("Attempted to merge data models where at least one model was null or they were the same model!");;

            var properties = existingModel.GetType().GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance);

            foreach(var prop in properties)
            {
                if(prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(newModel);
                    prop.SetValue(existingModel, value);
                }
            }
        }

        
    }
}
