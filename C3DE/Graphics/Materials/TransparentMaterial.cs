﻿using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Rendering;
using System.Runtime.Serialization;

namespace C3DE.Graphics.Materials
{
    [DataContract]
    public class TransparentMaterial : Material
    {
        public TransparentMaterial(string name = "Transparent Material")
            : base(name)
        {
            _hasAlpha = true;
        }

        protected override void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is ForwardRenderer || renderer is LightPrePassRenderer)
                m_ShaderMaterial = new ForwardTransparent(this);
            else if (renderer is DeferredRenderer)
                m_ShaderMaterial = new DeferredTransparent(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }
    }
}
