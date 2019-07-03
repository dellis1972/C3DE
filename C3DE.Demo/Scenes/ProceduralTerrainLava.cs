﻿using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scenes
{
    public class ProceduralTerrainLava : ProceduralTerrainBase
    {
        public ProceduralTerrainLava() : base("Procedural Terrain (Lava)") { }

        protected override void SetupScene()
        {
            _terrain.GetComponent<MeshRenderer>().Material.MainTexture = Application.Content.Load<Texture2D>("Textures/Terrain/Rock");

            // Lava
            var lavaTexture = Application.Content.Load<Texture2D>("Textures/Fluids/lava_texture");
            var lavaNormal = Application.Content.Load<Texture2D>("Textures/Fluids/lava_bump");
            var lava = GameObjectFactory.CreateLava(lavaTexture, lavaNormal, new Vector3(_terrain.Width * 0.5f));
            Add(lava);
        }
    }
}