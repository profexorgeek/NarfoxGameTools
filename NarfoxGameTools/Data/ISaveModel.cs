using System;

namespace NarfoxGameTools.Data
{
    /// <summary>
    /// A version of a ViewModel that can be persisted to disk.
    /// It should always contain a version and date. The Value field
    /// can be used to convert the save to some cash value in game if
    /// the object can not be deserialized in the current game version
    /// </summary>
    public interface ISaveModel
    {
        /// <summary>
        /// The game version that this was saved under
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The date this item was saved. Should usually be set
        /// before serialized to disk!
        /// </summary>
        DateTime SaveDate { get; set; }
    }
}
