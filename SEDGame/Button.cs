using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEDGame
{
    class Button
    {
        static Color DisabledColor = Color.Lerp(Color.White, Color.Transparent, 0.5f);

        private Rectangle _target;
        private Texture2D _texture;

        public bool Enabled = true;
        private bool _clicked = false;

        public event EventHandler OnClick;

        public Rectangle Target
        {
            get { return _target; }
            set { _target = value; }
        }

        public Button(Texture2D tex)
        {
            _target = Rectangle.Empty;
            _texture = tex;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(_texture, _target,  new Rectangle(_clicked ? 64 : 0, 0, 64, 64), (Enabled ? Color.White : DisabledColor));
        }

        public bool Input(MouseState ms, MouseState ls)
        {
            if (!Enabled)
                return false;

            if (!_target.Contains(ms.Position))
            {
                _clicked = false;

                return false;
            }

            if(_clicked && ms.LeftButton == ButtonState.Released)
            {
                _clicked = false;
                return true;
            }

            if (ms.LeftButton == ButtonState.Pressed && ls.LeftButton == ButtonState.Released)
            {
                OnClick(this, null);
                _clicked = true;

                return true;
            }

            return false;
        }
    }
}
