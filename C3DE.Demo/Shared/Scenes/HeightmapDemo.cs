﻿using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class HeightmapDemo : SimpleDemo
    {
        public HeightmapDemo() : base("Heightmap Terrain") { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            // Finally a terrain
            var terrainMaterial = new StandardTerrainMaterial();
            terrainMaterial.MainTexture = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_col");
            terrainMaterial.NormalMap = content.Load<Texture2D>("Textures/Terrain/Ground/Ground03_nrm");
            terrainMaterial.SandTexture = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_col");
            terrainMaterial.SandNormalMap = content.Load<Texture2D>("Textures/Terrain/Sand/Ground27_nrm");
            terrainMaterial.SnowTexture = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_col");
            terrainMaterial.SnownNormalMap = content.Load<Texture2D>("Textures/Terrain/Snow/Snow05_nrm");
            terrainMaterial.RockTexture = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_col");
            terrainMaterial.RockNormalMap = content.Load<Texture2D>("Textures/Terrain/Rock/Rock12_nrm");

            var terrainGo = GameObjectFactory.CreateTerrain();

            _scene.Add(terrainGo);

            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.LoadHeightmap("Textures/heightmap");
            terrain.Renderer.Material = terrainMaterial;

            var weightMap = terrain.GenerateWeightMap();
            terrainMaterial.WeightTexture = weightMap;
            terrainMaterial.Tiling = new Vector2(4);
            terrainGo.AddComponent<WeightMapViewer>();

            // With water !
            var waterTexture = content.Load<Texture2D>("Textures/Fluids/water");
            var bumpTexture = content.Load<Texture2D>("Textures/Fluids/wavesbump");
            var water = GameObjectFactory.CreateWater(waterTexture, bumpTexture, new Vector3(terrain.Width * 0.5f));
            water.Transform.Translate(0, 10.0f, 0);
            _scene.Add(water);

            // And fog
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.Skybox.FogSupported = true;
            RenderSettings.Skybox.OverrideSkyboxFog(FogMode.Exp2, 0.05f, 0, 0);

            var vrPlayerEnabler = _camera.AddComponent<VRPlayerEnabler>();
            vrPlayerEnabler.Position = new Vector3(0, water.Transform.Position.Y + 0.5f, 0);
        }
    }
}