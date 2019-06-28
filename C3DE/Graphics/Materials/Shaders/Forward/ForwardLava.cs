﻿using System;
using C3DE.Components;
using C3DE.Components.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Graphics.Materials.Shaders
{
    public class ForwardLava : ShaderMaterial
    {
        private LavaMaterial m_Material;
        private EffectPass m_PassAmbient;
        private EffectParameter m_EPView;
        private EffectParameter m_EPProjection;
        private EffectParameter m_EPTime;
        private EffectParameter m_EPMainTexture;
        private EffectParameter m_EPNormalTexture;
        private EffectParameter m_EPTextureTilling;
        private EffectParameter m_EPDiffuseColor;
        private EffectParameter m_EPWorld;
        private EffectParameter m_EPEmissiveIntensity;

        public bool EmissiveEnabled => false;

        public ForwardLava(LavaMaterial material)
        {
            m_Material = material;
        }

        public override void LoadEffect(ContentManager content)
        {
            _effect = content.Load<Effect>("Shaders/Forward/Lava");
            SetupParameters();
        }

        protected virtual void SetupParameters()
        {
            m_PassAmbient = _effect.CurrentTechnique.Passes["AmbientPass"];
            m_EPView = _effect.Parameters["View"];
            m_EPProjection = _effect.Parameters["Projection"];
            m_EPTime = _effect.Parameters["Time"];
            m_EPMainTexture = _effect.Parameters["MainTexture"];
            m_EPNormalTexture = _effect.Parameters["NormalTexture"];
            m_EPTextureTilling = _effect.Parameters["TextureTiling"];
            m_EPDiffuseColor = _effect.Parameters["DiffuseColor"];
            m_EPWorld = _effect.Parameters["World"];
            m_EPEmissiveIntensity = _effect.Parameters["EmissiveIntensity"];
        }

        public override void PrePass(Camera camera)
        {
            m_EPView.SetValue(camera.m_ViewMatrix);
            m_EPProjection.SetValue(camera.m_ProjectionMatrix);
        }

        public override void Pass(Renderer renderable)
        {
            m_EPTime.SetValue(Time.TotalTime * m_Material.Speed);
            m_EPMainTexture.SetValue(m_Material.MainTexture);
            m_EPNormalTexture.SetValue(m_Material.NormalTexture);
            m_EPTextureTilling.SetValue(m_Material.Tiling);
            m_EPDiffuseColor.SetValue(m_Material._diffuseColor);
            m_EPWorld.SetValue(renderable.Transform.m_WorldMatrix);
            m_EPEmissiveIntensity.SetValue(m_Material.EmissiveIntensity);
            m_PassAmbient.Apply();
        }
    }
}
