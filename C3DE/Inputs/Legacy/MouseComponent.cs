﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs
{
    public enum MouseButton
    {
        Left = 0, Middle, Right, Any
    }

    public class MouseComponent : GameComponent
    {
        private MouseState _mouseState;
        private MouseState _prevState;
        protected Vector2 _delta;

        #region Fields

        public int X
        {
            get { return _mouseState.X; }
        }

        public int Y
        {
            get { return _mouseState.Y; }
        }

        public int Wheel
        {
            get { return _mouseState.ScrollWheelValue - _prevState.ScrollWheelValue; }
        }

        public bool Moving
        {
            get { return (_mouseState.X != _prevState.X) || (_mouseState.Y != _prevState.Y); }
        }

        public Vector2 Delta
        {
            get { return _delta; }
        }

        /// <summary>
        /// Specifies whether the mouse is draggin
        /// </summary>
        /// <param name="button">The button to test</param>
        /// <returns>True if draggin then false</returns>
        public bool Drag(MouseButton button = MouseButton.Left)
        {
            return Down(button) && Moving;
        }

        /// <summary>
        /// Get the mouse position on screen
        /// </summary>
        public Vector2 Position
        {
            get { return new Vector2(_mouseState.X, _mouseState.Y); }
        }

        public Vector2 PreviousPosition
        {
            get { return new Vector2(_prevState.X, _prevState.Y); }
        }

        #endregion

        public MouseComponent(Game game)
            : base(game)
        {
            _mouseState = Mouse.GetState();
            _prevState = _mouseState;
            _delta = Vector2.Zero;
        }

        public override void Update(GameTime gameTime)
        {
            // Update states
            _prevState = _mouseState;
            _mouseState = Mouse.GetState();

            // Delta
            if (Screen.LockCursor)
            {
                _delta.X = (_mouseState.X - Screen.WidthPerTwo);
                _delta.Y = (_mouseState.Y - Screen.HeightPerTwo);
            }
            else
            {
                _delta.X = (_mouseState.X - _prevState.X);
                _delta.Y = (_mouseState.Y - _prevState.Y);
            }

            base.Update(gameTime);
        }

        public virtual void SetPosition(int x, int y)
        {
            Mouse.SetPosition(x, y);
        }

        #region Mouse click

        public virtual bool JustClicked(MouseButton button = MouseButton.Left)
        {
            bool clicked = false;

            if (button == MouseButton.Left)
                clicked = _mouseState.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed;
            else if (button == MouseButton.Middle)
                clicked = _mouseState.MiddleButton == ButtonState.Released && _prevState.MiddleButton == ButtonState.Pressed;
            else if (button == MouseButton.Right)
                clicked = _mouseState.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed;

            return clicked;
        }

        public virtual bool Down(MouseButton button)
        {
            return MouseButtonState(button, ButtonState.Pressed);
        }

        public virtual bool Up(MouseButton button = MouseButton.Left)
        {
            return MouseButtonState(button, ButtonState.Released);
        }

        protected virtual bool MouseButtonState(MouseButton button, ButtonState state)
        {
            bool result = false;

            switch (button)
            {
                case MouseButton.Left: result = _mouseState.LeftButton == state; break;
                case MouseButton.Middle: result = _mouseState.MiddleButton == state; break;
                case MouseButton.Right: result = _mouseState.RightButton == state; break;
            }

            return result;
        }

        #endregion
    }
}