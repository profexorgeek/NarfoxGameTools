namespace Narfox.Services;

/// <summary>
/// Provides a seeded and unseeded random instance that can be
/// used for deterministic or non-deterministic procedural generation
/// </summary>
public class RandomService
{
    static RandomService? instance;
    static object padlock = new Object();
    Random seeded;
    Random unseeded;

    /// <summary>
    /// A random instance that is based on the seed provided
    /// during initialization
    /// </summary>
    public Random SeededRandom => seeded;

    /// <summary>
    /// A random instance that can be used without affecting
    /// the master seeded random instance
    /// </summary>
    public Random UnseededRandom => unseeded;

    /// <summary>
    /// A singleton instance property, limited to a single character
    /// for easy use. Cannot be accessed until Initialize has
    /// been called on this service.
    /// </summary>
    public static RandomService I
    {
        get
        {
            if(instance == null)
            {
                throw new InvalidOperationException("This service has not been initialized");
            }
            return instance;
        }
    }

    /// <summary>
    /// A static initializer that prepares this singleton service for
    /// use. If this method is not called before use, an exception
    /// will be thrown when trying to access the instance property.
    /// </summary>
    /// <param name="seed"></param>
    public static void Initialize(int seed = 1234567890)
    {
        lock(padlock)
        {
            if(instance == null)
            {
                instance = new RandomService(seed);
            }
        }
    }

    /// <summary>
    /// The constructor, initializes the random instances
    /// </summary>
    /// <param name="seed"></param>
    private RandomService(int seed)
    {
        seeded = new Random(seed);
        unseeded = new Random(seeded.Next());
    }
}


