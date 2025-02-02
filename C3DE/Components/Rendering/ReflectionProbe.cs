﻿using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    public class ReflectionProbe : Component
    {
        public enum RenderingMode
        {
            Backed = 0, Realtime
        }

        internal TextureCube _reflectionTexture;
        internal Camera _camera;
        private float _fov = 75;
        private float _nearClip = 1.0f;
        private float _farClip = 500.0f;
        private int _size = 64;
        private RenderingMode _renderingMode = RenderingMode.Backed;

        public float Radius { get; set; } = 10;

        public RenderingMode Mode
        {
            get => _renderingMode;
            set
            {
                _renderingMode = value;
                Dirty = true;
            }
        }

        public float FieldOfView
        {
            get => _fov;
            set
            {
                _fov = value;
                UpdateMatrix();
            }
        }

        public float NearClip
        {
            get => _nearClip;
            set
            {
                _nearClip = value;

                // The near clip have to be lower than the far clip.
                if (_farClip < _nearClip)
                    _farClip = _nearClip + 1;

                UpdateMatrix();
            }
        }

        public float FarClip
        {
            get => _farClip;
            set
            {
                _farClip = value;
                UpdateMatrix();
            }
        }

        public int Resolution
        {
            get => _size;
            set
            {
                _size = value;
                UpdateRenderTargets();
                UpdateMatrix();
            }
        }

        public TextureCube ReflectionMap => _reflectionTexture;

        public bool Dirty { get; set; } = true;

        public override void Start()
        {
            base.Start();

            _camera = AddComponent<Camera>();

            if (Camera.Main == _camera)
                Camera.Main = null;

            UpdateRenderTargets();
            UpdateMatrix();
        }

        private void UpdateMatrix()
        {
            _camera._clearColor = Color.Transparent;
            _camera.Near = NearClip;
            _camera.Far = FarClip;
            _camera.AspectRatio = 1.0f;
            _camera.FieldOfView = FieldOfView;
            _camera.Setup(_transform.Position, Vector3.Forward, Vector3.Up);
            _camera.Update();

            Dirty = true;
        }

        private void UpdateRenderTargets()
        {
            _camera.RenderTarget?.Dispose();
            _camera.RenderTarget = new RenderTarget2D(Application.GraphicsDevice, _size, _size);

            _reflectionTexture?.Dispose();
            _reflectionTexture = new TextureCube(Application.GraphicsDevice, _size, false, SurfaceFormat.Color);

            Dirty = true;
        }

        public Vector3 GetCameraRotation(CubeMapFace face)
        {
            if (face == CubeMapFace.PositiveX)
                return new Vector3(0.0f, MathHelper.ToRadians(-90), 0.0f);
            else if (face == CubeMapFace.NegativeX)
                return new Vector3(0.0f, MathHelper.ToRadians(90), 0.0f);

            else if (face == CubeMapFace.PositiveY)
                return new Vector3(MathHelper.ToRadians(90), 0.0f, 0.0f);
            else if (face == CubeMapFace.NegativeY)
                return new Vector3(MathHelper.ToRadians(-90), 0.0f, 0.0f);

            else if (face == CubeMapFace.PositiveZ)
                return new Vector3(0.0f, MathHelper.ToRadians(180.0f), 0.0f);
            else if (face == CubeMapFace.NegativeZ)
                return new Vector3(0.0f, MathHelper.ToRadians(0.0f), 0.0f);

            return Vector3.Zero;
        }

        public void AutoAssign(Renderer[] renderers, bool forceRendererCompute)
        {
            var sphere = new BoundingSphere(Transform.Position, Radius);

            foreach (var renderer in renderers)
            {
                if (forceRendererCompute)
                    renderer.ComputeBoundingInfos();

                if (renderer.boundingSphere.Intersects(sphere))
                {
                    var std = renderer.Material as StandardMaterial;
                    if (std == null)
                        continue;

                    std.ReflectionMap = _reflectionTexture;

                    if (std.ReflectionIntensity == 0)
                        std.ReflectionIntensity = 0.25f;
                }
            }
        }
    }
}
