using FlatRedBall;
using Microsoft.Xna.Framework;
using NarfoxGameTools.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Extensions
{
    public static class CameraExtensions
    {
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
