﻿using C3DE.Components.Colliders;
using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Components.Renderers
{
    /// <summary>
    /// An abstract class to define a renderable object.
    /// </summary>
    public abstract class Renderer : Component
    {
        protected internal BoundingSphere boundingSphere;
        protected internal BoundingBox boundingBox;
        protected internal int materialIndex;

        /// <summary>
        /// Indicates whether the object can cast shadow. 
        /// </summary>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Indicates whether the object can receive shadow.
        /// </summary>
        public bool ReceiveShadow { get; set; }

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }

        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Gets the main material.
        /// </summary>
        public Material Material
        {
            get { return materialIndex > -1 ? Scene.current.materials[materialIndex] : Scene.current.defaultMaterial; }
            set { materialIndex = value.Index; }
        }

        /// <summary>
        /// Create a renderable component which can cast and receive shadow. No material is assigned.
        /// </summary>
        public Renderer()
            : base()
        {
            CastShadow = true;
            ReceiveShadow = true;
            boundingSphere = new BoundingSphere();
            boundingBox = new BoundingBox();
            materialIndex = 0;
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
                boundingSphere.Center = transform.Position;
        }

        /// <summary>
        /// Compute colliders size.
        /// </summary>
        protected void UpdateColliders()
        {
            var colliders = GetComponents<Collider>();
            var size = colliders.Length;

            if (size > 0)
            {
                for (var i = 0; i < size; i++)
                    colliders[i].Compute();
            }
        }

        /// <summary>
        /// Compute the internal bouding sphere of the collider, it's required by the shadow generator.
        /// </summary>
        public abstract void ComputeBoundingInfos();

        /// <summary>
        /// Draw the content of the component.
        /// </summary>
        /// <param name="device"></param>
        public abstract void Draw(GraphicsDevice device);
		
		public override SerializedCollection Serialize()
        {
            var data = base.Serialize();
            data.IncreaseCapacity(3);
            data.Add("CastShadow", CastShadow.ToString());
            data.Add("ReceiveShadow", ReceiveShadow.ToString());
            data.Add("MaterialIndex", materialIndex.ToString());
            return data;
        }

        public override void Deserialize(SerializedCollection data)
        {
            base.Deserialize(data);
            CastShadow = bool.Parse(data["CastShadow"]);
			ReceiveShadow = bool.Parse(data["ReceiveShadow"]);
			materialIndex = int.Parse(data["MaterialIndex"]);
        }
    }
}