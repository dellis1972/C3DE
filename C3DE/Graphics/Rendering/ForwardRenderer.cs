﻿using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Materials.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics.Rendering
{
    /// <summary>
    /// The renderer is responsible to render a scene with a camera view to the screen.
    /// </summary>
    public class ForwardRenderer : BaseRenderer
    {
        private DepthRenderer m_DepthRenderer;

        public DepthRenderer DepthRenderer => m_DepthRenderer;

        public ForwardRenderer(GraphicsDevice graphics)
           : base(graphics)
        {
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);
            RebuildRenderTargets();
            m_DepthRenderer = new DepthRenderer();
        }

        public override void Dispose(bool disposing)
        {
            if (!m_IsDisposed)
            {
                if (disposing)
                    DisposeObject(m_SceneRenderTargets);

                m_IsDisposed = true;
            }
        }

        /// <summary>
        /// Renders the scene with the specified camera.
        /// Render order:
        /// 1 - Shadow maps
        /// 2 - Objects
        /// 3 - Post Processing
        /// 4 - UI
        /// </summary>
        /// <param name="scene">The scene to render.</param>
        /// <param name="camera">The camera to use for render.</param>
        public override void Render(Scene scene)
        {
            if (scene == null || scene?.cameras.Count == 0)
                return;

            var camera = scene.cameras[0];

            if (scene._reflectionProbes.Count > 0)
            {
                for (var i = 0; i < scene._reflectionProbes.Count; i++)
                    scene._reflectionProbes[i].Draw(this);
            }

            RebuildRenderTargets();
            RenderShadowMaps(scene);

            if (m_VREnabled)
            {
                // Apply camera parenting
                var cameraParent = Matrix.Identity;
                var parent = camera._transform.Parent;
                if (parent != null)
                    cameraParent = parent._worldMatrix;

                for (var eye = 0; eye < 2; eye++)
                {
                    camera._projectionMatrix = m_VRService.GetProjectionMatrix(eye);
                    camera._viewMatrix = m_VRService.GetViewMatrix(eye, cameraParent);
                    RenderSceneForCamera(scene, camera, m_SceneRenderTargets[eye]);
                }

                m_VRService.SubmitRenderTargets(m_SceneRenderTargets[0], m_SceneRenderTargets[1]);
                DrawVRPreview(0);
                RenderUI(scene.Behaviours);
            }
            else
                RenderSceneForCamera(scene, camera, m_SceneRenderTargets[0]);
        }

        protected virtual void RenderSceneForCamera(Scene scene, Camera camera, RenderTarget2D renderTarget)
        {
            if (m_DepthRenderer.Enabled)
                m_DepthRenderer.Draw(m_graphicsDevice);

            m_graphicsDevice.SetRenderTarget(renderTarget);

            var renderToRT = camera.RenderTarget != null;
            if (renderToRT)
                m_graphicsDevice.SetRenderTarget(camera.RenderTarget);

            m_graphicsDevice.Clear(camera._clearColor);

            RenderObjects(scene, camera);
            RenderPostProcess(scene.postProcessPasses, renderTarget);

            if (renderToRT)
                return;

            if (!m_VREnabled)
            {
                RenderToBackBuffer();
                RenderUI(scene.Behaviours);
            }
        }

        /// <summary>
        /// Renders renderable objects
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        protected void RenderObjects(Scene scene, Camera camera)
        {
            m_graphicsDevice.DepthStencilState = DepthStencilState.Default;
            m_graphicsDevice.BlendState = BlendState.Opaque;

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;
            IMultipassLightingMaterial multiLightShader;
            var lights = scene.lights;
            var lightCount = lights.Count;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];
                material = scene.renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(m_graphicsDevice);
                    continue;
                }

                shader = material.m_ShaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i]);
                renderer.Draw(m_graphicsDevice);

                // Lightpass
                if (shader is IMultipassLightingMaterial)
                {
                    multiLightShader = (IMultipassLightingMaterial)shader;

                    m_graphicsDevice.BlendState = BlendState.Additive;

                    for (var l = 0; l < lightCount; l++)
                    {
                        multiLightShader.LightPass(renderer, lights[l]);
                        renderer.Draw(m_graphicsDevice);
                    }

                    m_graphicsDevice.BlendState = BlendState.Opaque;
                }
            }
        }

        public override void RenderReflectionProbe(Camera camera)
        {
            var renderTargets = m_graphicsDevice.GetRenderTargets();

            m_graphicsDevice.SetRenderTarget(camera.RenderTarget);
            m_graphicsDevice.Clear(camera._clearColor);

            m_graphicsDevice.DepthStencilState = DepthStencilState.Default;
            m_graphicsDevice.BlendState = BlendState.Opaque;

            var scene = Scene.current;
            var ambientColor = scene.RenderSettings.ambientColor;
            scene.RenderSettings.ambientColor = Color.White.ToVector3();

            if (scene.RenderSettings.Skybox.Enabled)
                scene.RenderSettings.Skybox.Draw(m_graphicsDevice, camera);

            var renderCount = scene.renderList.Count;

            Renderer renderer;
            Material material;
            ShaderMaterial shader;
            var lights = scene.lights;
            var lightCount = lights.Count;

            // Pass, Update matrix, material attributes, etc.
            for (var i = 0; i < renderCount; i++)
            {
                renderer = scene.renderList[i];

                var isMeshRenderer = renderer is MeshRenderer;
                var isModelRenderer = renderer is ModelRenderer;

                if (!isMeshRenderer && !isModelRenderer)
                    continue;

                material = scene.renderList[i].Material;

                // A specific renderer that uses its own draw logic.
                if (material == null)
                {
                    renderer.Draw(m_graphicsDevice);
                    continue;
                }

                shader = material.m_ShaderMaterial;

                // Ambient pass
                shader.PrePass(camera);
                shader.Pass(scene.RenderList[i]);
                renderer.Draw(m_graphicsDevice);
            }

            scene.RenderSettings.ambientColor = ambientColor;

            m_graphicsDevice.SetRenderTargets(renderTargets);
        }
    }
}
