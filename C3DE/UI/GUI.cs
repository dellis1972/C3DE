﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace C3DE.UI
{
    public class GUI
    {
        private SpriteBatch _spriteBatch;
        private Vector2 _cacheVec2;
        private Rectangle _cacheRect;

        public static GUISkin Skin { get; set; }

        public GUI(SpriteBatch spriteBatch)
        {
            _spriteBatch = spriteBatch;
        }

        public void LoadContent(ContentManager content)
        {
            Skin = new GUISkin();
            Skin.LoadContent(content);
        }

        public void Box(Rectangle rect, string text)
        {
            _spriteBatch.Draw(Skin.Box, rect, Color.White);

            _cacheVec2 = Skin.Font.MeasureString(text);
            _cacheVec2.X = (rect.X + rect.Width / 2) - (_cacheVec2.X / 2);
            _cacheVec2.Y = rect.Y + Skin.Margin;

            Label(_cacheVec2, text);
        }

        public bool Button(Rectangle rect, string text)
        {
            var index = 0;

            if (rect.Contains(Input.Mouse.Position))
            {
                index = 1;
                if (Input.Mouse.Clicked())
                    index = 2;
            }

            _spriteBatch.Draw(Skin.Buttons[index], rect, Color.White);

            _cacheVec2 = Skin.Font.MeasureString(text);
            _cacheVec2.X = (rect.X + rect.Width / 2) - (_cacheVec2.X / 2);
            _cacheVec2.Y = (rect.Y + rect.Height / 2) - (_cacheVec2.Y / 2);
            Label(_cacheVec2, text);

            return index == 2;
        }

        public bool Checkbox(Rectangle rect, string text, bool isChecked)
        {
            var index = isChecked ? 2 : 0;

            // Draw the first square
            _cacheRect.X = rect.X;
            _cacheRect.Y = rect.Y;
            _cacheRect.Width = rect.Height;
            _cacheRect.Height = rect.Height;

            _spriteBatch.Draw(Skin.Checkbox[0], _cacheRect, Color.White);

            // Draw the text
            text = WrapText(Skin.Font, text, rect.Width);
            _cacheVec2 = Skin.Font.MeasureString(text);
            _cacheVec2.X = rect.X + rect.Height + Skin.Margin;
            _cacheVec2.Y = rect.Y + rect.Height / 2 - _cacheVec2.Y / 2;

            Label(_cacheVec2, text);

            // If checked, draw the check square
            if (isChecked)
            {
                _cacheRect.X = rect.X + 1;
                _cacheRect.Y = rect.Y + 1;
                _cacheRect.Width = rect.Height - 2;
                _cacheRect.Height = rect.Height - 2;

                _spriteBatch.Draw(Skin.Checkbox[2], _cacheRect, Color.White);
            }

            if (rect.Contains(Input.Mouse.Position))
            {
                index = 1;

                if (Input.Mouse.Clicked())
                    index = isChecked ? 0 : 2;
     
                _cacheRect.X = rect.X + 1;
                _cacheRect.Y = rect.Y + 1;
                _cacheRect.Width = rect.Height - 2;
                _cacheRect.Height = rect.Height - 2;

                _spriteBatch.Draw(Skin.Checkbox[index], _cacheRect, Color.White);

                // Restore previous state if not clicked
                if (isChecked && index > 0)
                    index = 2;
            }

            return index == 2;
        }

        public void Label(Vector2 position, string text, float scale = 1.0f, float rotation = 0.0f)
        {
            _cacheVec2.X = scale;
            _cacheVec2.Y = scale;
            _spriteBatch.DrawString(Skin.Font, text, position, Skin.TextColor, rotation, Vector2.Zero, _cacheVec2, SpriteEffects.None, 1);
        }

        private string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');

            StringBuilder sb = new StringBuilder();

            float lineWidth = 0.0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;
            Vector2 wordSize = Vector2.Zero;

            foreach (string word in words)
            {
                wordSize = spriteFont.MeasureString(word);

                if (lineWidth + wordSize.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += wordSize.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = wordSize.X + spaceWidth;
                }
            }

            return sb.ToString();
        }
    }
}