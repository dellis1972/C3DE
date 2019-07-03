﻿using C3DE.Components.Rendering;
using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace C3DE.Components.Physics
{
    /// <summary>
    /// A sphere collider component used to handle collisions by sphere.
    /// </summary>
    [DataContract]
    public class SphereCollider : Collider
    {
        private BoundingSphere _sphere;

        /// <summary>
        /// Gets the bounding sphere.
        /// </summary>
        [DataMember]
        public BoundingSphere Sphere
        {
            get { return _sphere; }
            set { _sphere = value; }
        }

        /// <summary>
        /// Create an empty sphere collider.
        /// </summary>
        public SphereCollider()
            : base()
        {
            _sphere = new BoundingSphere();
        }

        public override void Update()
        {
            if (!_gameObject.IsStatic)
                _sphere.Center = _transform.LocalPosition;
        }

        public override void Compute()
        {
            var renderable = GetComponent<Renderer>();
            if (renderable != null)
                _sphere = renderable.boundingSphere;
        }

        public override bool Collides(Collider other)
        {
            if (other is SphereCollider)
                return _sphere.Intersects((other as SphereCollider).Sphere);

            if (other is BoxCollider)
                return _sphere.Intersects((other as BoxCollider).BoundingBox);

            return false;
        }

        public override float? IntersectedBy(ref Ray ray)
        {
            return ray.Intersects(_sphere);
        }
    }
}
