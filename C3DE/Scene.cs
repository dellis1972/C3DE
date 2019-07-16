﻿using C3DE.Components;
using C3DE.Components.Physics;
using C3DE.Components.Lighting;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Jitter.Collision;
using Jitter;

namespace C3DE
{
    public class SerializedScene
    {
        public RenderSettings RenderSettings { get; set; }
        public GameObject[] GameObjects { get; set; }
        public Material[] Materials { get; set; }
    }

    public struct RaycastInfo
    {
        public Vector3 Position;
        public Ray Ray;
        public Collider Collider;
        public float Distance;
    }

    /// <summary>
    /// The scene is responsible to store scene objects, components.
    /// </summary>
    public class Scene : GameObject
    {
        public static Scene current { get; internal set; }

        private List<Component> _componentsToDestroy;
        private bool _needRemoveCheck;

        internal protected Material defaultMaterial;
        internal protected List<GameObject> gameObjects;
        internal protected List<Renderer> renderList;
        internal protected List<PlanarReflection> planarReflections;
        internal protected List<Material> materials;
        internal protected List<Effect> effects;
        internal protected Dictionary<int, int> materialsEffectIndex;
        internal protected List<Collider> colliders;
        internal protected List<Camera> cameras;
        internal protected List<Light> lights;
        internal protected List<Behaviour> scripts;
        internal protected List<GameObject> prefabs;
        internal protected List<PostProcessPass> postProcessPasses;

        internal protected CollisionSystem _physicsCollisionSystem;
        internal protected World _physicsWorld;

        public RenderSettings RenderSettings { get; private set; }

        /// <summary>
        /// Gets the collection of renderable scene objects.
        /// </summary>
        public List<Renderer> RenderList => renderList;

        /// <summary>
        /// Gets materials.
        /// </summary>
        public List<Material> Materials => materials;

        /// <summary>
        /// Gets colliders.
        /// </summary>
        public List<Collider> Colliders => colliders;

        /// <summary>
        /// Gets lights.
        /// </summary>
        public List<Light> Lights => lights;

        /// <summary>
        /// Gets cameras.
        /// </summary>
        public List<Camera> Cameras => cameras;

        /// <summary>
        /// Gets scripts.
        /// </summary>
        public List<Behaviour> Behaviours => scripts;

        /// <summary>
        /// Gets prefabs.
        /// </summary>
        public List<GameObject> Prefabs => prefabs;

        public List<PostProcessPass> PostProcessPasses => postProcessPasses;

        /// <summary>
        /// The root scene object which contains all scene objects.
        /// </summary>
        public Scene()
            : base()
        {
            Name = "Scene-" + Guid.NewGuid();
            _transform.Root = _transform;
            gameObjects = new List<GameObject>();
            _scene = this;
            renderList = new List<Renderer>(10);
            materials = new List<Material>(5);
            effects = new List<Effect>(5);
            materialsEffectIndex = new Dictionary<int, int>(5);
            colliders = new List<Collider>(5);
            cameras = new List<Camera>(1);
            scripts = new List<Behaviour>(5);
            lights = new List<Light>(2);
            prefabs = new List<GameObject>();
            postProcessPasses = new List<PostProcessPass>();
            _componentsToDestroy = new List<Component>();
            _needRemoveCheck = false;
            defaultMaterial = new UnlitMaterial();
            RenderSettings = new RenderSettings();
            planarReflections = new List<PlanarReflection>();
            _physicsCollisionSystem = new CollisionSystemSAP();
            _physicsWorld = new World(_physicsCollisionSystem);
         }

        public Scene(string name)
            : this()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;
        }

        #region Lifecycle

        /// <summary>
        /// Initialize the scene. This method is called whenever the scene is used by
        /// the SceneManager.
        /// </summary>
        /// <param name="content"></param>
        public override void Initialize()
        {
            _initialized = true;

            RenderSettings.Skybox.LoadContent(Application.Content);

            for (var i = 0; i < materials.Count; i++)
                materials[i].LoadContent(Application.Content);

            for (int i = 0; i < gameObjects.Count; i++)
                gameObjects[i].Initialize();
        }

        /// <summary>
        /// Update all scene object.
        /// </summary>
        public override void Update()
        {
            base.Update();

            _physicsWorld.Step(Time.DeltaTime, true);

            // First - Check if we need to remove some components.
            if (_needRemoveCheck)
            {
                for (int i = 0, l = _componentsToDestroy.Count; i < l; i++)
                {
                    if (_componentsToDestroy[i] != null)
                    {
                        CheckComponent(_componentsToDestroy[i], ComponentChangeType.Remove);
                        _componentsToDestroy[i] = null;
                    }
                }

                _needRemoveCheck = false;
            }

            // Third - Safe update
            for (int i = 0; i < gameObjects.Count; i++)
                if (gameObjects[i].Enabled)
                    gameObjects[i].Update();
        }

        /// <summary>
        /// Unload the scene.
        /// </summary>
        public virtual void Unload()
        {
            foreach (Behaviour script in Behaviours)
                script.OnDestroy();

            foreach (GameObject gameObject in gameObjects)
                gameObject.Dispose();

            foreach (Material material in materials)
                material.Dispose();

            foreach (PostProcessPass pass in postProcessPasses)
                pass.Dispose();

            Clear();
        }

        /// <summary>
        /// Clean the scene.
        /// </summary>
        protected void Clear()
        {
            renderList.Clear();
            materials.Clear();
            effects.Clear();
            materialsEffectIndex.Clear();
            colliders.Clear();
            cameras.Clear();
            lights.Clear();
            scripts.Clear();
            gameObjects.Clear();
            prefabs.Clear();
            postProcessPasses.Clear();
            planarReflections.Clear();
            _componentsToDestroy.Clear();
            _needRemoveCheck = false;
        }

        #endregion

        #region GameObjects/Components management

        public override bool Add(GameObject gameObject)
        {
            return Add(gameObject, false);
        }

        public bool Add(GameObject gameObject, bool noCheck)
        {
            bool canAdd = base.Add(gameObject);

            if (canAdd)
            {
                if (!gameObject.IsPrefab)
                {
                    gameObjects.Add(gameObject);
                    gameObject.Scene = this;
                    gameObject.Transform.Root = _transform;

                    if (gameObject.Enabled)
                    {
                        CheckComponents(gameObject, ComponentChangeType.Add);
                        gameObject.PropertyChanged += OnGameObjectPropertyChanged;
                        gameObject.ComponentChanged += OnGameObjectComponentChanged;
                    }

                    if (_initialized && !gameObject.Initialized)
                        gameObject.Initialize();
                }
                else
                    AddPrefab(gameObject);
            }

            return canAdd;
        }

        /// <summary>
        /// Add a prefab only before the scene is started.
        /// </summary>
        /// <param name="prefab"></param>
        protected void AddPrefab(GameObject prefab)
        {
            if (!prefabs.Contains(prefab))
                prefabs.Add(prefab);
        }

        protected void RemovePrefab(GameObject prefab)
        {
            if (prefabs.Contains(prefab))
                prefabs.Remove(prefab);
        }

        /// <summary>
        /// Check all components of a scene object to update all list of the scene.
        /// </summary>
        /// <param name="gameObject">The scene object.</param>
        /// <param name="type">Type of change.</param>
        protected void CheckComponents(GameObject gameObject, ComponentChangeType type)
        {
            for (int i = 0; i < gameObject.Components.Count; i++)
                CheckComponent(gameObject.Components[i], type);
        }

        /// <summary>
        /// Check a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="type"></param>
        protected void CheckComponent(Component component, ComponentChangeType type)
        {
            if (component is Renderer)
            {
                var renderable = component as Renderer;

                if (type == ComponentChangeType.Add)
                    AddRenderer(renderable);

                else if (type == ComponentChangeType.Remove)
                    RemoveRenderer(renderable);
            }

            else if (component is PlanarReflection)
            {
                var planar = component as PlanarReflection;

                if (type == ComponentChangeType.Add)
                    AddPlanarReflection(planar);
                else if (type == ComponentChangeType.Remove)
                    RemovePlanarReflection(planar);
            }

            else if (component is Behaviour)
            {
                var script = component as Behaviour;

                if (type == ComponentChangeType.Add)
                    AddScript(script);
                else if (type == ComponentChangeType.Remove)
                    RemoveScript(script);
            }

            else if (component is Collider)
            {
                var collider = component as Collider;

                if (type == ComponentChangeType.Add)
                    AddCollider(collider);
                else if (type == ComponentChangeType.Remove)
                    RemoveCollider(collider);
            }

            else if (component is Camera)
            {
                var camera = component as Camera;

                if (type == ComponentChangeType.Add && !cameras.Contains(camera))
                    AddCamera(camera);
                else if (type == ComponentChangeType.Remove)
                    RemoveCamera(camera);
            }

            else if (component is Light)
            {
                var light = component as Light;

                if (type == ComponentChangeType.Add)
                    AddLight(light);
                else if (type == ComponentChangeType.Remove)
                    RemoveLight(light);
            }
        }

        private void OnGameObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.Name == "Enabled")
            {
                var gameObject = (GameObject)sender;
                if (gameObject.Enabled)
                {
                    CheckComponents(gameObject, ComponentChangeType.Add);
                    gameObject.ComponentChanged += OnGameObjectComponentChanged;
                }
                else
                {
                    CheckComponents(gameObject, ComponentChangeType.Remove);
                    gameObject.ComponentChanged -= OnGameObjectComponentChanged;
                }
            }
        }

        /// <summary>
        /// Called when a component is added to a registered scene object.
        /// It's actually used to update the render list.
        /// </summary>
        /// <param name="sender">The scene object which as added or removed a component.</param>
        /// <param name="e">An object which contains the component and a flag to know if it's added or removed.</param>
        private void OnGameObjectComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (e.ChangeType == ComponentChangeType.Update)
            {
                if (e.PropertyName == "Enabled")
                    CheckComponent(e.Component, e.Component.Enabled ? ComponentChangeType.Add : ComponentChangeType.Remove);
            }
            else
                CheckComponent(e.Component, e.ChangeType);
        }

        #endregion

        #region Add / Get / Remove materials

        /// <summary>
        /// Add a new material.
        /// </summary>
        /// <param name="material"></param>
        internal protected void AddMaterial(Material material)
        {
            if (!materials.Contains(material))
            {
                materials.Add(material);

                if (Initialized)
                    material.LoadContent(Application.Content);
            }
        }

        /// <summary>
        /// Remove a material.
        /// </summary>
        /// <param name="material"></param>
        internal protected void RemoveMaterial(Material material)
        {
            if (materials.Contains(material))
            {
                materials.Remove(material);
                material.Dispose();
            }
        }

        #endregion

        #region Add/Remove components

        internal protected int AddCamera(Camera camera)
        {
            var index = cameras.IndexOf(camera);

            if (index == -1)
            {
                cameras.Add(camera);
                cameras.Sort();
                index = cameras.Count - 1;

                if (Camera.Main == null)
                    Camera.Main = camera;
            }

            return index;
        }

        protected void AddPlanarReflection(PlanarReflection planar)
        {
            if (planarReflections.Contains(planar))
                return;

            planarReflections.Add(planar);
            planarReflections.Sort();
        }

        protected void AddRenderer(Renderer renderer)
        {
            if (renderList.Contains(renderer))
                return;

            renderList.Add(renderer);
            renderList.Sort();
        }

        protected void AddLight(Light light)
        {
            if (lights.Contains(light))
                return;

            lights.Add(light);
            lights.Sort();
        }

        protected void AddCollider(Collider collider)
        {
            if (!colliders.Contains(collider))
                colliders.Add(collider);
        }

        protected void AddScript(Behaviour script)
        {
            if (!scripts.Contains(script))
                scripts.Add(script);
        }

        protected void RemoveRenderer(Renderer renderable)
        {
            if (renderList.Contains(renderable))
                renderList.Remove(renderable);
        }

        protected void RemovePlanarReflection(PlanarReflection planar)
        {
            if (planarReflections.Contains(planar))
                planarReflections.Remove(planar);
        }

        protected void RemoveScript(Behaviour script)
        {
            if (scripts.Contains(script))
                scripts.Remove(script);
        }

        protected void RemoveLight(Light light)
        {
            if (lights.Contains(light))
                lights.Remove(light);
        }

        protected void RemoveCollider(Collider collider)
        {
            if (colliders.Contains(collider))
                colliders.Remove(collider);
        }

        protected void RemoveCamera(Camera camera)
        {
            if (cameras.Contains(camera))
                cameras.Remove(camera);
        }

        #endregion

        #region Destroy GameObjects/Components

        private int GetFirstNullRemovedComponent()
        {
            for (int i = 0, l = _componentsToDestroy.Count; i < l; i++)
            {
                if (_componentsToDestroy[i] == null)
                    return i;
            }

            return -1;
        }

        public override bool Remove(GameObject gameObject)
        {
            return Remove(gameObject, false);
        }

        public bool Remove(GameObject gameObject, bool noCheck = false)
        {
            bool canRemove = base.Remove(gameObject);

            if (canRemove)
                DestroyObject(gameObject, noCheck);

            return canRemove;
        }

        public void DestroyObject(GameObject gameObject, bool noCheck = false)
        {
            for (int i = 0, l = gameObject.Components.Count; i < l; i++)
                this.DestroyComponent(gameObject.Components[i]);

            gameObjects.Remove(gameObject);
        }

        public void DestroyComponent(Component component)
        {
            var index = GetFirstNullRemovedComponent();

            if (index > -1)
                _componentsToDestroy[index] = component;
            else
                _componentsToDestroy.Add(component);

            _needRemoveCheck = true;
        }

        #endregion

        #region Add/Remove PostProcess

        public void Add(PostProcessPass pass)
        {
            if (!postProcessPasses.Contains(pass))
            {
                postProcessPasses.Add(pass);
                pass.Initialize(Application.Content);
            }
        }

        public void Remove(PostProcessPass pass)
        {
            if (postProcessPasses.Contains(pass))
                postProcessPasses.Remove(pass);
        }

        #endregion

        #region Search methods

        public static GameObject FindById(string id)
        {
            if (current != null)
            {
                for (int i = 0; i < current.gameObjects.Count; i++)
                    if (current.gameObjects[i].Id == id)
                        return current.gameObjects[i];
            }
            return null;
        }

        public static GameObject[] FindGameObjectsById(string id)
        {
            var gameObjects = new List<GameObject>();

            if (current != null)
            {
                for (int i = 0; i < current.gameObjects.Count; i++)
                    if (current.gameObjects[i].Id == id)
                        gameObjects.Add(current.gameObjects[i]);
            }

            return gameObjects.ToArray();
        }

        public static T FindObjectOfType<T>() where T : Component
        {
            var scripts = new List<T>();

            if (current != null)
            {
                foreach (GameObject so in current.gameObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        return components[0];
                }
            }

            return default(T);
        }

        public static T[] FindObjectsOfType<T>() where T : Component
        {
            var scripts = new List<T>();

            if (current != null)
            {
                foreach (GameObject so in current.gameObjects)
                {
                    var components = so.GetComponents<T>();
                    if (components.Length > 0)
                        scripts.AddRange(components);
                }
            }

            return scripts.ToArray();
        }

        public Material GetMaterialByName(string name)
        {
            foreach (var mat in materials)
                if (mat.Name == name)
                    return mat;
            return null;
        }

        #endregion

        #region Collisions detection

        public Collider Collides(Collider collider)
        {
            for (int i = 0, l = colliders.Count; i < l; i++)
            {
                if (collider.Collides(colliders[i]))
                    return colliders[i];
            }

            return null;
        }

        #endregion

        #region Raycast

        private bool Raycast(Ray ray, float distance = 1000.0f)
        {
            float? val;

            for (int i = 0, l = colliders.Count; i < l; i++)
            {
                val = colliders[i].IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                    return true;
            }

            return false;
        }

        public bool Raycast(Ray ray, float distance, out RaycastInfo info)
        {
            info = new RaycastInfo();
            RaycastInfo[] infos;
            RaycastAll(ray, distance, out infos);

            var size = infos.Length;
            if (size > 0)
            {
                var min = float.MaxValue;
                var index = -1;

                for (int i = 0; i < size; i++)
                {
                    if (infos[i].Distance < min)
                    {
                        min = infos[i].Distance;
                        index = i;
                    }
                }

                if (index > -1)
                    info = infos[index];
            }

            return size > 0;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance = 1000.0f)
        {
            return Raycast(new Ray(origin, direction), distance);
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float distance, out RaycastInfo info)
        {
            return Raycast(new Ray(origin, direction), distance, out info);
        }

        public bool RaycastAll(Ray ray, float distance, out RaycastInfo[] raycastInfos)
        {
            var infos = new List<RaycastInfo>();

            for (int i = 0, l = colliders.Count; i < l; i++)
                TestCollision(ref ray, colliders[i], distance, infos);

            raycastInfos = infos.ToArray();

            return raycastInfos.Length > 0;
        }

        private void TestCollision(ref Ray ray, Collider collider, float distance, List<RaycastInfo> infos)
        {
            if (collider.IsPickable)
            {
                var val = collider.IntersectedBy(ref ray);

                if (val.HasValue && val.Value <= distance)
                {

                    infos.Add(new RaycastInfo()
                    {
                        Position = ray.Position,
                        Collider = collider,
                        Distance = val.Value,
                        Ray = ray
                    });
                }
            }
        }

        public bool RaycastAll(Vector3 origin, Vector3 direction, float distance, out RaycastInfo[] infos)
        {
            return RaycastAll(new Ray(origin, direction), distance, out infos);
        }

        #endregion
    }
}
