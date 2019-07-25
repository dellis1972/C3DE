﻿using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using C3DE.Graphics.Shaders.Forward;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials
{
    public class StandardTerrainMaterial : Material
    {
        public Texture2D GrassNormalMap { get; set; }
        public Texture2D SnowTexture { get; set; }
        public Texture2D SnownNormalMap { get; set; }
        public Texture2D SandTexture { get; set; }
        public Texture2D SandNormalMap { get; set; }
        public Texture2D RockTexture { get; set; }
        public Texture2D RockNormalMap { get; set; }
        public Texture2D WeightTexture { get; set; }

        public int SpecularPower { get; set; } = 16;
        public float SpecularIntensity { get; set; } = 1;
        public Color SpecularColor { get; set; } = new Color(0.6f, 0.6f, 0.6f);

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer)
                _shaderMaterial = new ForwardStandardTerrain(this);
            else if (renderer is DeferredRenderer)
                _shaderMaterial = new DeferredStandardTerrain(this);

            _shaderMaterial.LoadEffect(Application.Content);
        }
    }
}
