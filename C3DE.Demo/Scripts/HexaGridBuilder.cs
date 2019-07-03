﻿using C3DE.Components;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts
{
    public class HexaGridBuilder : Behaviour
    {
        private ModelRenderer _gridPrefab;
        private float _hexWidth;
        private float _hexDepth;

        public int GridWidth { get; set; }
        public int GridDepth { get; set; }
        public float TileScale { get; set; }
        public float Margin { get; set; }

        public HexaGridBuilder()
            : base()
        {
            GridWidth = 10;
            GridDepth = 10;
            Margin = 0.86f;
            TileScale = 1.0f;
        }

        public override void Start()
        {
            var go = GameObjectFactory.CreateXNAModel(Application.Content, "Models/HexGrid");
            _gridPrefab = go.GetComponent<ModelRenderer>();
            _gridPrefab.Transform.LocalScale = new Vector3(TileScale, 0.5f * TileScale, TileScale);
            _gridPrefab.Transform.LocalRotation = new Vector3(0, MathHelper.Pi / 6, 0);
            _gridPrefab.Material = new StandardMaterial();
            _gridPrefab.Material.MainTexture = Application.Content.Load<Texture2D>("Models/hexagone_basic");
            _gridPrefab.Enabled = false;
            _gameObject.Scene.Add(go);

            _hexWidth = _gridPrefab.BoundingSphere.Radius * 2 * 0.85f * Margin;
            _hexDepth = _gridPrefab.BoundingSphere.Radius * 2 * Margin;

            GenerateHexaGrid();
        }

        private Vector3 CalculateInitialPosition()
        {
            Vector3 position = new Vector3();
            position.X = -_hexWidth * GridWidth / 2.0f + _hexWidth / 2.0f;
            position.Y = 0;
            position.Z = GridDepth / 2.0f * _hexDepth - _hexDepth / 2.0f;
            return position;
        }

        public Vector3 GetWorldCoordinate(float x, float z)
        {
            Vector3 position = CalculateInitialPosition();

            float offset = 0;

            if (z % 2 != 0)
                offset = _hexWidth / 2.0f;

            var px = position.X + offset + x * _hexWidth;
            var pz = position.Z - z * _hexDepth * 0.75f;

            return new Vector3(px, 0, pz);
        }

        private void GenerateHexaGrid()
        {
            GameObject cache = null;

            var waterMaterial = _gridPrefab.Material;

            var groundMaterial = new StandardMaterial();
            groundMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/hexagone_green");

            var montainMaterial = new StandardMaterial();
            montainMaterial.MainTexture = Application.Content.Load<Texture2D>("Models/hexagone_brown");
            montainMaterial.DiffuseColor = Color.Red;

            int rand = 0;
            ModelRenderer mRenderer = null;

            for (int z = 0; z < GridDepth; z++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    rand = RandomHelper.Range(0, 10);

                    cache = Scene.Instanciate(_gridPrefab.GameObject);
                    cache.Transform.LocalPosition = GetWorldCoordinate(x, z);
                    cache.Transform.Parent = _transform;

                    mRenderer = cache.GetComponent<ModelRenderer>();

                    if (rand % 2 == 0)
                    {
                        mRenderer.Material = groundMaterial;
                        cache.Transform.LocalScale  += new Vector3(0, 0.5f, 0);
                    }
                    else if (rand % 5 == 0)
                    {
                        mRenderer.Material = montainMaterial;
                        cache.Transform.LocalScale += new Vector3(0.0f, 1.5f, 0.0f);
                    }

                    cache.Transform.SetLocalPosition(null, _gridPrefab.BoundingSphere.Radius * cache.Transform.LocalScale.Y / 2, null);
                }
            }
        }
    }
}
