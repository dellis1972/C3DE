﻿using Microsoft.Xna.Framework;
using System;

namespace C3DE.Utils
{
    public class RandomHelper
    {
        private static Random _random = new Random();

        public static int Value => Range(int.MinValue, int.MaxValue);

        public static float ValueF => Range(0, 1);

        /// <summary>
        /// Gets a random float value between min and max.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <returns>A random float value.</returns>
        public static float Range(float min, float max) => (float)(_random.NextDouble() * (max - min) + min);

        /// <summary>
        /// Gets a random integer value between min and max.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <returns>An random integer value.</returns>
        public static int Range(int min, int max) => _random.Next(min, max);

        /// <summary>
        /// Gets a random Vector2.
        /// </summary>
        /// <param name="minX">Min X value.</param>
        /// <param name="minY">Min Y value.</param>
        /// <param name="maxX">Max X value.</param>
        /// <param name="maxY">Max Y value.</param>
        /// <returns>A random Vector2.</returns>
        public static Vector2 GetVector2(float minX, float minY, float maxX, float maxY)
        {
            return new Vector2(
                Range(minX, maxX),
                Range(minY, maxY));
        }

        /// <summary>
        /// Gets a random Vector3.
        /// </summary>
        /// <param name="minX">Min X value.</param>
        /// <param name="minY">Min Y value.</param>
        /// <param name="minZ">Min Z value.</param>
        /// <param name="maxX">Max X value.</param>
        /// <param name="maxY">Max Y value.</param>
        /// <param name="maxZ">Max Z value.</param>
        /// <returns>A random Vector3.</returns>
        /// <returns></returns>
        public static Vector3 GetVector3(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            return new Vector3(
                Range(minX, maxX),
                Range(minY, maxY),
                Range(minZ, maxZ));
        }

        /// <summary>
        /// Gets a random color.
        /// </summary>
        /// <param name="alpha">Alpha amount</param>
        /// <returns>A random color.</returns>
        public static Color GetColor(float alpha = 1.0f)
        {
            return new Color(
                Range(0.0f, 1.0f),
                Range(0.0f, 1.0f),
                Range(0.0f, 1.0f), 
                alpha < 0 ? Range(0.0f, 1.0f) : alpha);
        }
    }
}
