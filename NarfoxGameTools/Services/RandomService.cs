using FlatRedBall;
using System;

namespace NarfoxGameTools.Services
{
    /// <summary>
    /// Normalizes random object access so it can be a
    /// "seed-critical" random object or just the FRB
    /// default.
    /// </summary>
    public static class RandomService
    {
        public static Random Random { get; set; } = FlatRedBallServices.Random;
    }
}
