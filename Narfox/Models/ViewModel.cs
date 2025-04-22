﻿using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Narfox.Models;

/// <summary>
/// This attribute enables dependency tracking for bound properties
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
public class DependsOnAttribute : Attribute
{
    public string ParentProperty { get; set; }

    public DependsOnAttribute(string parentPropertyName)
    {
        ParentProperty = parentPropertyName;
    }

    /// <summary>
    /// Creates a new DependsOnAttribute which access the owner's subproperty.
    /// </summary>
    /// <param name="owner">The owner of the property.</param>
    /// <param name="parentPropertyName">The property inside of the owner.</param>
    public DependsOnAttribute(string owner, string parentPropertyName)
    {
        ParentProperty = owner + "." + parentPropertyName;
    }

    public override string ToString()
    {
        return $"Depends on {ParentProperty}";
    }
}

/// <summary>
/// Enum defining the possible behaviors when a ViewModel attempts to retrieve a value of a different type than the property stored.
/// </summary>
public enum TypeMismatchBehavior
{
    /// <summary>
    /// Error is ignored - the default value for the type is returned.
    /// </summary>
    IgnoreError,
    /// <summary>
    /// An InvalidCastException is thrown.
    /// </summary>
    ThrowException
}

/// <summary>
/// Helper class to hold old and new values for a changing property.
/// 
/// See:
/// https://stackoverflow.com/questions/47723876/how-to-capture-old-value-and-new-value-in-inotifypropertychanged-implementation
/// 
/// </summary>
public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
{
    public virtual object OldValue { get; private set; }
    public virtual object NewValue { get; private set; }

    public PropertyChangedExtendedEventArgs(string propertyName, object oldValue,
           object newValue)
           : base(propertyName)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Standard implementation of an observable view model.
/// This class implements the INotifyPropertyChanged interface 
/// so it is suitable for use as a base class for any ViewModel
/// in an MVVM architecture. Typically this is used with Gum and 
/// FlatRedBall.Forms.
/// </summary>
public class ViewModel : INotifyPropertyChanged
{
    #region Fields
    Dictionary<string, List<string>> notifyRelationships = new Dictionary<string, List<string>>();
    private Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();
    private Dictionary<string, object> oldValueDictionary = new Dictionary<string, object>();
    private List<string> dependsOnOwners;

    /// <summary>
    /// The behavior of the ViewModel when the Get function attempts to retrieve a value of a different type 
    /// than the property stored. If set to Ignore, then no error is raised and the default for the type is returned.
    /// If this value is set to ThrowException, then an InvalidCastException is thrown.
    /// </summary>
    public static TypeMismatchBehavior DefaultTypeMismatchBehavior = TypeMismatchBehavior.IgnoreError;

    Dictionary<INotifyPropertyChanged, string> ObjectToNameDictionary = new Dictionary<INotifyPropertyChanged, string>();
    #endregion


    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion


    #region Properties
    [JsonIgnore]
    public int PropertyChangedSubscriptionCount => PropertyChanged?.GetInvocationList().Length ?? 0;

    #endregion


    public ViewModel()
    {
        var derivedType = GetType();

        var properties = derivedType.GetRuntimeProperties();

        foreach (var property in properties)
        {
            var attributes = property.GetCustomAttributes(true);

            string propertyName = property.Name;
            foreach (var uncastedAttribute in attributes)
            {
                if (uncastedAttribute is DependsOnAttribute attribute)
                {
                    string parentProperty = attribute.ParentProperty;

                    List<string> childrenProps = null;
                    if (notifyRelationships.ContainsKey(parentProperty) == false)
                    {
                        childrenProps = new List<string>();
                        notifyRelationships[parentProperty] = childrenProps;
                    }
                    else
                    {
                        childrenProps = notifyRelationships[parentProperty];
                    }

                    if (parentProperty.Contains("."))
                    {
                        var owner = parentProperty.Substring(0, parentProperty.IndexOf('.'));

                        if (dependsOnOwners == null)
                        {
                            dependsOnOwners = new List<string>();
                        }

                        if (!dependsOnOwners.Contains(owner))
                        {
                            dependsOnOwners.Add(owner);
                        }
                    }

#if DEBUG
                    if (parentProperty == propertyName)
                    {
                        throw new InvalidOperationException(
                            $"The property {propertyName} should not depend on itself");
                    }
#endif

                    childrenProps.Add(propertyName);
                }
            }
        }

    }

    object GetValueThroughDictionaryOrReflection(string propertyName)
    {
        if (propertyDictionary.ContainsKey(propertyName))
        {
            return propertyDictionary[propertyName];
        }
        else
        {
            var propInfo = GetType().GetProperty(propertyName);
            return propInfo?.GetValue(this);
        }
    }

    // This cannot be a local func with closures or else -= will not work.
    // To guarantee that, let's move this out to class scope. Then += will work fine.
    void HandleDependsOwnerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var senderAsNotifyPropertyChanged = sender as INotifyPropertyChanged;
        string senderName = "";
        if (senderAsNotifyPropertyChanged != null && ObjectToNameDictionary.ContainsKey(senderAsNotifyPropertyChanged))
        {
            senderName = ObjectToNameDictionary[senderAsNotifyPropertyChanged];
        }
        if (e is PropertyChangedExtendedEventArgs eExtended)
        {
            NotifyPropertyChanged($"{senderName}.{e.PropertyName}", eExtended.OldValue, eExtended.NewValue);
        }
        else
        {
            NotifyPropertyChanged($"{senderName}.{e.PropertyName}");
        }
    }

    public void SetPropertyChanged(string propertyName, Action action)
    {
        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                action();
            }
        };
    }



    protected T Get<T>([CallerMemberName] string propertyName = null)
    {
        T toReturn = default;

        if (propertyName != null && propertyDictionary.ContainsKey(propertyName))
        {
            object uncasted = null;
            try
            {
                uncasted = propertyDictionary[propertyName];
                toReturn = (T)uncasted;
            }
            catch (InvalidCastException)
            {
                if (DefaultTypeMismatchBehavior == TypeMismatchBehavior.ThrowException)
                {
                    throw new InvalidCastException($"The property {propertyName} is of type {typeof(T)} but the inner object is of type {uncasted?.GetType()}");
                }
                // if it fails, then just return default T because the type may have changed:
                toReturn = default;
            }
            catch (Exception)
            {
                if (DefaultTypeMismatchBehavior == TypeMismatchBehavior.ThrowException)
                {
                    throw;
                }
                // if it fails, then just return default T because the type may have changed:
                toReturn = default;
            }
        }

        return toReturn;
    }

    protected bool Set<T>(T propertyValue, [CallerMemberName] string propertyName = null)
    {

        var oldValue = Get<T>(propertyName);

        if (propertyValue is INotifyCollectionChanged collection)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= CollectionChangedInternal;
            }
            collection.CollectionChanged += CollectionChangedInternal;
        }

        bool didSet = SetWithoutNotifying(propertyValue, propertyName, oldValue);

        if (didSet)
        {
            NotifyPropertyChanged(propertyName, oldValue, propertyValue);
        }

        return didSet;

        // Careful, this causes event accumulation. Need to solve this!!
        void CollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// This method is similar to Set but it will propagate changed events up through parent
    /// VMs. This is good for bubbling events upwards.
    /// 
    /// For example, you have a viewmodel structure like Editor.SelectedTile.PositionX, if
    /// you subscribe to PropertyChanged events on Editor, you will not have the event fire
    /// if PositionX changes if you are just using Set. If you use SetAndPropagate, an event
    /// will bubble up with the PropertyName "SelectedTile.PositionX".
    /// </summary>
    /// <typeparam name="T">The type of propertyValue, usually inferred</typeparam>
    /// <param name="propertyValue">The new value</param>
    /// <param name="propertyName">The name of the property, automatically passed</param>
    /// <returns></returns>
    protected bool SetAndPropagate<T>(T propertyValue, [CallerMemberName] string propertyName = null)
    {
        var oldValue = Get<T>(propertyName);

        // Unhook from old nested PropertyChanged
        if (oldValue is INotifyPropertyChanged oldNotify)
        {
            oldNotify.PropertyChanged -= NestedPropertyChanged;
        }

        // Unhook from old collection events
        if (oldValue is INotifyCollectionChanged oldCollection)
        {
            oldCollection.CollectionChanged -= CollectionChangedInternal;
        }

        // Hook into new nested PropertyChanged
        if (propertyValue is INotifyPropertyChanged newNotify)
        {
            newNotify.PropertyChanged += NestedPropertyChanged;
        }

        // Hook into new collection events
        if (propertyValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += CollectionChangedInternal;
        }

        bool didSet = SetWithoutNotifying(propertyValue, propertyName, oldValue);

        if (didSet)
        {
            NotifyPropertyChanged(propertyName, oldValue, propertyValue);
        }

        return didSet;

        void NestedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Bubble full path: "Property.SubProperty"
            NotifyPropertyChanged($"{propertyName}.{e.PropertyName}");
        }

        void CollectionChangedInternal(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Sets the underlying property without notifying any subscribers. This can be used for initial setting
    /// or in rare cases when properties should be set without any 
    /// </summary>
    /// <typeparam name="T">The type of the property being set.</typeparam>
    /// <param name="propertyValue">The new value.</param>
    /// <param name="propertyName">The name of the property to set - typically used with nameof.</param>
    /// <param name="oldValue">The old value, used to determine if a value should be assigned. If the old value matches the new value, then the property is not assigned and the return value is false.</param>
    /// <returns>Whether the value was set. This is true if the old property did not exist, or if the old value does not match the new value. If this is the initial set, then this value is ignored.</returns>
    protected bool SetWithoutNotifying<T>(T propertyValue, string propertyName, T oldValue)
    {
        var didSet = false;


        if (propertyDictionary.ContainsKey(propertyName))
        {
            if (EqualityComparer<T>.Default.Equals(oldValue, propertyValue) == false)
            {
                propertyDictionary[propertyName] = propertyValue;
                didSet = true;
            }
        }
        else
        {
            propertyDictionary[propertyName] = propertyValue;

            // Even though the user is setting a new value, we want to make sure it's
            // not the same:
            var defaultValue = default(T);
            var isSettingDefault =
                EqualityComparer<T>.Default.Equals(defaultValue, propertyValue);

            didSet = isSettingDefault == false;


        }


        return didSet;
    }

    protected void ChangeAndNotify<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(property, value) == false)
        {
            property = value;
            NotifyPropertyChanged(propertyName);
        }
    }

    protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null, object oldValue = null, object newValue = null)
    {
        if (PropertyChanged != null)
        {
            var args = new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue);
            PropertyChanged(this, args);
        }

        if (notifyRelationships.ContainsKey(propertyName))
        {
            var childPropertyNames = notifyRelationships[propertyName];

            foreach (var childPropertyName in childPropertyNames)
            {
                // This is going to be on "this" so we can use the old and new values passed, I believe...
                var newChildPropertyValue = GetValueThroughDictionaryOrReflection(childPropertyName);
                object oldChildPropertyValue = newValue == newChildPropertyValue
                    // The old value is the actual old value passed on to the parameter, so we can just use that:
                    ? oldValue
                    // we don't know the old value...
                    : null;

                var shouldNotify = true;
                if (oldValueDictionary.ContainsKey(childPropertyName))
                {
                    var lastValue = oldValueDictionary[childPropertyName];

                    //if (EqualityComparer<T>.Default.Equals(oldValue, propertyValue) == false)
                    if (lastValue == null && newChildPropertyValue == null)
                    {
                        shouldNotify = false;
                    }
                    else if (lastValue == null || newChildPropertyValue == null)
                    {
                        shouldNotify = true;
                    }
                    else
                    {
                        shouldNotify = !lastValue.Equals(newChildPropertyValue);
                    }
                }
                oldValueDictionary[childPropertyName] = newChildPropertyValue;

                if (shouldNotify)
                {
                    NotifyPropertyChanged(childPropertyName, oldChildPropertyValue, newChildPropertyValue);
                }
            }
        }

        if (dependsOnOwners?.Contains(propertyName) == true)
        {
            var withDot = propertyName + ".";
            foreach (var relationship in notifyRelationships)
            {
                if (relationship.Key.StartsWith(withDot))
                {
                    foreach (var childPropertyName in relationship.Value)
                    {
                        object newChildValue = null;
                        object oldChildValue = null;
                        // Jan 26 2023 LateBinder is busted... Not sure why, Joel prob doesn't know either...
                        //if (oldValue != null)
                        //{
                        //    oldChildValue = LateBinder.GetValueStatic(oldValue, childPropertyName);
                        //}
                        //if (newValue != null)
                        //{
                        //    newChildValue = LateBinder.GetValueStatic(newValue, childPropertyName);
                        //}

                        NotifyPropertyChanged(childPropertyName, oldChildValue, newChildValue);
                    }
                }
            }

            #region Internal Methods

            SubscribeToEventsOnNewProperty(newValue, propertyName, oldValue);



            void SubscribeToEventsOnNewProperty<T>(T _newValue, string _propertyName, T _oldValue)
            {
                var isDependsOwner = dependsOnOwners?.Contains(_propertyName) == true;
                if (isDependsOwner && _oldValue is INotifyPropertyChanged asNotifyPropertyChanged)
                {
                    if (ObjectToNameDictionary.ContainsKey(asNotifyPropertyChanged))
                    {
                        ObjectToNameDictionary.Remove(asNotifyPropertyChanged);
                        asNotifyPropertyChanged.PropertyChanged -= HandleDependsOwnerPropertyChanged;
                    }
                }

                if (isDependsOwner && _newValue is INotifyPropertyChanged asNotifyPropertyChanged2)
                {
                    if (ObjectToNameDictionary.ContainsKey(asNotifyPropertyChanged2) == false)
                    {
                        ObjectToNameDictionary[asNotifyPropertyChanged2] = _propertyName;
                        asNotifyPropertyChanged2.PropertyChanged += HandleDependsOwnerPropertyChanged;
                    }
                }
            }

            #endregion
            // This could have changed based on a different value, so since we now have a new dependensOnOnwer,
            // we should also look at updating this value:

        }
    }



    public virtual void ClearPropertyChangedEvents()
    {
        PropertyChanged = null;
    }
}