using FlatRedBall;
using Microsoft.Xna.Framework;
using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Extensions
{
    /// <summary>
    /// Extensions for the FRB Camera class
    /// </summary>
    public static class CameraExtensions
    {
        /// <summary>
        /// Gets the "zoom position" of a 3D camera as a
        /// percentage assuming that 100% would be the
        /// pixel perfect distance
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        public static float GetZoomPercentage(this Camera cam)
        {
            var pixelPerfectZ = cam.GetZDistanceForPixelPerfect();
            return cam.Position.Z / pixelPerfectZ;
        }

        /// <summary>
        /// Gets a random position within view of the camera.
        /// Can specify a Z position for 3D cameras and a custom
        /// random object. If no random object is specified, it will
        /// use the RandomService's random object.
        /// 
        /// Padding allows you to ensure that the whole object is within
        /// the camera view, not just it's origin point.
        /// </summary>
        /// <param name="cam">The camera to use</param>
        /// <param name="z">The Z index, defaults to 0</param>
        /// <param name="padding">How much to pad, or restrict, the camera's viewport</param>
        /// <param name="rand">The random object to use, defaults to use RandomService if left null</param>
        /// <returns></returns>
        public static Vector2 RandomPositionInView(this Camera cam, float z = 0, float padding = 0, Random rand = null)
        {
            rand = rand ?? RandomService.Random;

            var x = rand.InRange(cam.AbsoluteLeftXEdgeAt(z) + padding,
                cam.AbsoluteRightXEdgeAt(z) - padding);

            var y = rand.InRange(cam.AbsoluteBottomYEdgeAt(z) + padding,
                cam.AbsoluteTopYEdgeAt(z) - padding);

            return new Vector2(x, y);
        }
    }
}
