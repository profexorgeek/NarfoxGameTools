using FlatRedBall.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Entities
{
    /// <summary>
    /// An IEntity item can take and/or deal damage.
    /// 
    /// It can be targetable for movement or attacking
    /// and may have multiple states, such as spawning 
    /// or dead, where it cannot be targeted.
    /// 
    /// It has position and may have velocity (inherited
    /// from IPositionable), which allows
    /// IEntity objects to interact with each other
    /// </summary>
    public interface IGameEntity : IPositionable
    {
        /// <summary>
        /// The IPositionable interface does not enforce rotation.
        /// This allows AI and other code to be written against an
        /// object's rotation.
        /// </summary>
        float RotationZ { get; set; }

        /// <summary>
        /// Should return whether or not this object is in
        /// a targetable state
        /// </summary>
        bool Targetable { get; }

        /// <summary>
        /// The amount of damage this object can deal
        /// </summary>
        float Damage { get; }

        /// <summary>
        /// If this object was owned by another object, for instance
        /// if a shot is owned by a ship, it should return the owning
        /// ship instead of itself because the targeted entity may want
        /// to track the objects attacking it.
        /// 
        /// If the object is not owned by anything else, it should return itself.
        /// </summary>
        IGameEntity RootDamageSource { get; }

        /// <summary>
        /// Usually called when two objects have collided.
        /// 
        /// Checks whether damage can be dealt by the source. Status fx
        /// and other things should be applied in this method before
        /// actually affecting health
        /// </summary>
        /// <param name="amount">The amount of damage to be dealt</param>
        /// <param name="source">The source of the damage</param>
        /// <returns></returns>
        bool TryTakeDamage(float amount, IGameEntity source = null);

        /// <summary>
        /// Usually called by collision methods.
        /// 
        /// This allows the object to track the targets it has
        /// dealt and prevent itself from damaging a single target
        /// multiple times.
        /// </summary>
        /// <param name="target">The target that was damaged.</param>
        /// <returns></returns>
        void NotifyDamageDealt(IGameEntity target);

        /// <summary>
        /// Usually called by collision methods.
        /// 
        /// This checks if the damage dealer can damage the
        /// damage receiver. This is important for shots that
        /// aren't destroyed on impact so they can keep a list
        /// of objects they have already damaged and limit
        /// damage dealt to a single collision event.
        /// </summary>
        /// <param name="target">The target that will take damage</param>
        /// <returns>A true/false value depending on whether damage can be dealt</returns>
        bool CanDealDamageToTarget(IGameEntity target);
    }
}
