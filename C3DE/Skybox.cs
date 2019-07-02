﻿using C3DE.Components;
using C3DE.Graphics;
using C3DE.Graphics.Materials.Shaders;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Rendering;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE
{
    [DataContract]
    public class Skybox
    {
        private ShaderMaterial m_ShaderMaterial;
        private Matrix m_World;
        private Matrix _scaleMatrix;
        private CubeMesh m_Geometry;
        private TextureCube m_MainTexture;
        private RasterizerState m_SkyboxRasterizerState;
        private RasterizerState m_CurrentRasterizerState;
        private Vector4 m_CustomFogData;
        private bool m_OverrideFog;

        public TextureCube Texture
        {
            get { return m_MainTexture; }
            set { m_MainTexture = value; }
        }

        public Matrix WorldMatrix => m_World;

        [DataMember]
        public bool FogSupported { get; set; } = false;

        [DataMember]
        public bool Enabled { get; set; }

        public bool OverrideFog => m_OverrideFog;
        public Vector4 CustomFogData => m_CustomFogData;

        public Skybox()
        {
            m_Geometry = new CubeMesh();
            m_World = Matrix.Identity;
            _scaleMatrix = Matrix.CreateScale(1.0f);
            m_SkyboxRasterizerState = new RasterizerState();
            m_SkyboxRasterizerState.CullMode = CullMode.None;
        }

        protected void SetupShaderMaterial(BaseRenderer renderer)
        {
            if (renderer is DeferredRenderer)
                m_ShaderMaterial = new DeferredSkybox(this);
            else
                m_ShaderMaterial = new ForwardSkybox(this);

            m_ShaderMaterial.LoadEffect(Application.Content);
        }

        public void LoadContent(ContentManager content)
        {
            var engine = Application.Engine;
            SetupShaderMaterial(engine.Renderer);
            engine.RendererChanged += SetupShaderMaterial;
        }

        public void OverrideSkyboxFog(FogMode mode, float density, float start, float end)
        {
            m_CustomFogData.X = (float)mode;
            m_CustomFogData.Y = density;
            m_CustomFogData.Z = start;
            m_CustomFogData.W = end;
            m_OverrideFog = mode != FogMode.None;
        }

        public void Generate(GraphicsDevice device, Texture2D[] textures, float size = 250.0f)
        {
            if (textures.Length != 6)
                throw new Exception("The array of texture names must contains 6 elements.");

            m_Geometry.Size = new Vector3(size);
            m_Geometry.Build();
            m_Geometry.ComputeNormals();

            m_MainTexture = new TextureCube(device, textures[0].Width, false, SurfaceFormat.Color);
            Color[] textureData;

            for (int i = 0; i < 6; i++)
            {
                textureData = new Color[textures[i].Width * textures[i].Height];
                textures[i].GetData<Color>(textureData);
                m_MainTexture.SetData<Color>((CubeMapFace)i, textureData);
            }

            Enabled = true;
        }

        public void Generate(GraphicsDevice device, ContentManager content, string[] textureNames, float size = 250.0f)
        {
            var textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
                textures[i] = content.Load<Texture2D>(textureNames[i]);

            Generate(device, textures, size);
        }

        public void Generate(float size = 250.0f)
        {
            var skyTop = TextureFactory.CreateColor(new Color(168, 189, 255), 64, 64);
            var skySide = TextureFactory.CreateGradiant(new Color(168, 189, 255), Color.White, 64, 64);
            var skyBottom = TextureFactory.CreateColor(Color.White, 64, 64);

            Generate(Application.GraphicsDevice, new Texture2D[] {
                skySide,
                skySide,
                skyTop,
                skyBottom,
                skySide,
                skySide
            }, size);
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            m_CurrentRasterizerState = device.RasterizerState;
            device.RasterizerState = m_SkyboxRasterizerState;

            m_World = _scaleMatrix * Matrix.CreateTranslation(camera.Transform.LocalPosition);

            m_ShaderMaterial.PrePass(camera);

            device.SetVertexBuffer(m_Geometry.VertexBuffer);
            device.Indices = m_Geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_Geometry.Indices.Length / 3);
            device.RasterizerState = m_CurrentRasterizerState;
        }

        public void DrawNoEffect(GraphicsDevice device)
        {
            m_CurrentRasterizerState = device.RasterizerState;
            device.RasterizerState = m_SkyboxRasterizerState;
            device.SetVertexBuffer(m_Geometry.VertexBuffer);
            device.Indices = m_Geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_Geometry.Indices.Length / 3);
            device.RasterizerState = m_CurrentRasterizerState;
        }
    }
}
