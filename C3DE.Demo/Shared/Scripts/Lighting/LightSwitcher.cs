﻿using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Demo.Scripts.Lighting
{
    public class LightSwitcher : Behaviour
    {
        private Rectangle _box;
        private Rectangle _btn1;
        private Rectangle _btn2;
        private Rectangle _btn3;
        private Rectangle _btn4;
        private Light _light;

        public bool LogPositionRotation { get; set; } = false;

        public override void Start()
        {
            SetBoxAlign(false);
            _light = GetComponent<Light>();

            GUI.Skin.Font = Application.Content.Load<SpriteFont>("Font/Default");
        }

        public void SetBoxAlign(bool left)
        {
            _box = new Rectangle(left ? 10 : Screen.VirtualWidth - 150, 10, 140, 200);
            _btn1 = new Rectangle(_box.X + 10, _box.Y + 30, _box.Width - 20, 30);
            _btn2 = new Rectangle(_box.X + 10, _btn1.Y + 40, _box.Width - 20, 30);
            _btn3 = new Rectangle(_box.X + 10, _btn2.Y + 40, _box.Width - 20, 30);
            _btn4 = new Rectangle(_box.X + 10, _btn3.Y + 40, _box.Width - 20, 30);
        }

        public override void OnGUI(GUI gui)
        {
            gui.Box(_box, "Lights");

            if (gui.Button(_btn1, "Ambiant"))
                _light.Type = LightType.Ambient;

            if (gui.Button(_btn2, "Directional"))
                _light.Type = LightType.Directional;

            if (gui.Button(_btn3, "Point"))
                _light.Type = LightType.Point;

            if (gui.Button(_btn4, "Spot"))
                _light.Type = LightType.Spot;

            if (LogPositionRotation)
                Debug.Log($"p: {_transform.Position} r: {_transform.Rotation}");
        }
    }
}
