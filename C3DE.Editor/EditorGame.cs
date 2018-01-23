using C3DE.UI;
using System;
using XNAGizmo;

namespace C3DE.Editor
{
    public class EditorGame : Engine
    {
        private MainMenu m_MainMenu;
        private GizmoComponent m_Gizmo;
        private EditorScene m_EditorScene;

        public EditorGame()
            : base("C3DE Editor", 1440, 900, false)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            m_MainMenu = new MainMenu(this);
            m_MainMenu.CommandSelected += OnCommandSelected;
            m_MainMenu.GameObjectSelected += OnGameObjectSelected;
            Components.Add(m_MainMenu);

            m_Gizmo = new GizmoComponent(this, GraphicsDevice);
            Components.Add(m_Gizmo);

            var hotkeys = new HotkeyManager(this);
            hotkeys.ActionKeyJustPressed += OnCommandSelected;
            Components.Add(hotkeys);

            GUI.Skin = new GUISkin("Font/Menu");
            GUI.Skin.LoadContent(Content);

            NewScene();
        }

        private void OnCommandSelected(string command)
        {
            switch (command)
            {
                case "New": NewScene(); break;
                case "Load": LoadScene(); break;
                case "Save": SaveScene(); break;
                case "Exit": Exit(); break;
                case "About": break;
                case "Copy": break;
                case "Cut": break;
                case "Past": break;
                case "Select All": break;
                default: break;
            }
        }

        private void OnGameObjectSelected(string name)
        {
            m_EditorScene.AddObject(name);
        }

        public void NewScene()
        {
            if (m_EditorScene != null)
            {
                m_Gizmo.Selection.Clear();
                m_EditorScene.GameObjectAdded -= OnGameObjectAdded;
                m_EditorScene.GameObjectSelected -= OnGameObjectSelected;
                Application.SceneManager.Remove(m_EditorScene);
            }

            m_EditorScene = new EditorScene();
            m_EditorScene.GameObjectAdded += OnGameObjectAdded;
            m_EditorScene.GameObjectSelected += OnGameObjectSelected;

            Application.SceneManager.Add(m_EditorScene);
            Application.SceneManager.LoadLevel(0);
        }

        private void OnGameObjectSelected(GameObject gameObject)
        {
        }

        private void OnGameObjectAdded(GameObject gameObject, bool removed)
        {
        }

        public void SaveScene()
        {
            SaveScene(""); // TODO: Display a dialog
        }

        public void LoadScene()
        {
            LoadScene(""); // TODO: Display a dialog
        }

        public bool SaveScene(string path)
        {
            var result = true;

            try
            {
                var serScene = new SerializedScene()
                {
                    Materials = m_EditorScene.Materials.ToArray(),
                    GameObjects = m_EditorScene.GetUsedSceneObjects(),
                    RenderSettings = m_EditorScene.RenderSettings
                };

                Serializer.Serialize(path, serScene);
            }
            catch (Exception ex)
            {
                result = false;
                Debug.Log(ex.Message);
            }

            return result;
        }

        public bool LoadScene(string path)
        {
            var result = true;

            try
            {
                var data = Serializer.Deserialize(path, typeof(SerializedScene));
                var serializedScene = data as SerializedScene;
                if (serializedScene != null)
                {
                    NewScene();

                    foreach (var so in serializedScene.GameObjects)
                    {
                        so.PostDeserialize();
                        m_EditorScene.Add(so);
                    }

                    m_EditorScene.RenderSettings.Set(serializedScene.RenderSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                result = false;
            }

            return result;
        }

#if !ANDROID && !NETFX_CORE
        static void Main(string[] args)
        {
            using (var game = new EditorGame())
                game.Run();
        }
#endif
    }
}
