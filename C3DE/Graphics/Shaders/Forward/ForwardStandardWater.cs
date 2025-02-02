﻿using C3DE.Graphics.Materials;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Shaders.Forward
{
    public class ForwardStandardWater : ForwardStandardBase
    {
        protected StandardWaterMaterial _material;
        private Vector3 _features;

        public ForwardStandardWater(StandardWaterMaterial material)
        {
            _material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            var shaderPath = "Shaders/Forward/StandardWater";

            if (GraphicsAPI == GraphicsAPI.OpenGL)
                shaderPath = "Shaders/Forward/OpenGL/StandardWater";

            _effect = content.Load<Effect>(shaderPath);
        }

        public override void Pass(ref Matrix worldMatrix, bool receiveShadow, bool drawInstanced)
        {
            _features.X = _material.NormalMap != null ? 1 : 0;
            _features.Y = _material.SpecularMap != null ? 1 : 0; 
            _features.Z = _material.ReflectionIntensity;

            _effect.Parameters["TotalTime"].SetValue(Time.TotalTime * _material.Speed);
            _effect.Parameters["TextureTiling"].SetValue(_material.Tiling);
            _effect.Parameters["World"].SetValue(worldMatrix);
            _effect.Parameters["AlbedoMap"].SetValue(_material.MainTexture);
            _effect.Parameters["DiffuseColor"].SetValue(_material._diffuseColor);
            _effect.Parameters["SpecularPower"].SetValue(_material.SpecularPower);
            _effect.Parameters["SpecularColor"].SetValue(_material.SpecularColor.ToVector3());
            _effect.Parameters["SpecularIntensity"].SetValue(_material.SpecularIntensity);
            _effect.Parameters["ReflectionIntensity"].SetValue(_material.ReflectionIntensity);
            _effect.Parameters["ReflectionMap"].SetValue(_material.ReflectionMap);
            _effect.Parameters["Alpha"].SetValue(_material.Alpha);

            if (GraphicsAPI == GraphicsAPI.Direct3D)
            {
                _effect.Parameters["ShadowEnabled"].SetValue(receiveShadow);
                _effect.Parameters["SpecularMap"].SetValue(_material.SpecularMap);
                _effect.Parameters["NormalMap"].SetValue(_material.NormalMap);
                _effect.Parameters["Features"].SetValue(_features);
            }

            _effect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
