﻿using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Extensions
{
    public static class SpriteExtensions
    {
        public static void Colorize(this Sprite sprite, 
            Color color,
            bool preserveAlpha = false,
            ColorOperation operation = ColorOperation.Modulate)
        {
            sprite.ColorOperation = operation;
            sprite.Red = (float)(color.R / 255f);
            sprite.Green = (float)(color.G / 255f);
            sprite.Blue = (float)(color.B / 255f);

            if (!preserveAlpha)
            {
                sprite.Alpha = (float)(color.A / 255f);
            }
        }
    }
}