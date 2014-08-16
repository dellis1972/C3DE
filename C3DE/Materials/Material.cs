﻿using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Materials
{
    public abstract class Material
    {
        private static int MaterialCounter = 0;

        protected internal Scene scene;
        protected Vector4 diffuseColor;
        protected Texture2D mainTexture;
        protected internal Effect effect;

        public int Id
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            set;
        }

        internal int Index
        {
            get { return scene.Materials.IndexOf(this); }
        }

        public Color DiffuseColor
        {
            get { return new Color(diffuseColor); }
            set { diffuseColor = value.ToVector4(); }
        }

        public Texture2D MainTexture
        {
            get { return mainTexture; }
            set { mainTexture = value; }
        }

        public Material(Scene mainScene)
        {
            diffuseColor = Color.White.ToVector4();
            scene = mainScene;
            scene.Add(this);
            Id = MaterialCounter++;
            Name = "Material_" + Id;
        }

        public abstract void LoadContent(ContentManager content);

        public abstract void PrePass();

        public abstract void Pass(RenderableComponent renderable);

        public void Dispose()
        {
            scene = null;
            mainTexture.Dispose();
            effect.Dispose();
        }
    }
}
