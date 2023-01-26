using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace OpenLaMulana.Entities
{
    public class NineSliceBox : IGameEntity
    {
        enum NSlice
        {
            TL,
            TM,
            TR,
            L,
            M,
            R,
            BL,
            BM,
            BR,
            MAX
        };

        int IGameEntity.Depth { get; set; } = (int)Global.DrawOrder.UI;

        Effect IGameEntity.ActiveShader => null;
        public bool LockTo30FPS { get; set; } = true;

        Vector2 Position = Vector2.Zero;
        private Sprite _treasureIcon = null;
        private string _treasureName = String.Empty;
        private string _takenString = String.Empty;
        Sprite[] _boxSprites = new Sprite[(int)NSlice.MAX];
        private bool _visible = true;
        private int _boxWidth = 1;
        private int _boxHeight = 1;
        private int _windowGrowthRate = 0;
        private int _windowGrowthTimer = -1;
        private bool _doneDrawing = false;

        public NineSliceBox(Vector2 position, Sprite treasureIcon, string treasureName)
        {
            Texture2D itemTex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _boxSprites[(int)NSlice.TL] = Global.TextureManager.Get8x8Tile(itemTex, 0, 17, Vector2.Zero);
            _boxSprites[(int)NSlice.TM] = Global.TextureManager.Get8x8Tile(itemTex, 3, 17, Vector2.Zero);
            _boxSprites[(int)NSlice.TR] = Global.TextureManager.Get8x8Tile(itemTex, 1, 17, Vector2.Zero);
            _boxSprites[(int)NSlice.L] = Global.TextureManager.Get8x8Tile(itemTex, 2, 17, Vector2.Zero);
            _boxSprites[(int)NSlice.M] = Global.TextureManager.Get8x8Tile(itemTex, 28, 8, Vector2.Zero);
            _boxSprites[(int)NSlice.R] = Global.TextureManager.Get8x8Tile(itemTex, 2, 18, Vector2.Zero);
            _boxSprites[(int)NSlice.BL] = Global.TextureManager.Get8x8Tile(itemTex, 0, 18, Vector2.Zero);
            _boxSprites[(int)NSlice.BM] = Global.TextureManager.Get8x8Tile(itemTex, 3, 18, Vector2.Zero);
            _boxSprites[(int)NSlice.BR] = Global.TextureManager.Get8x8Tile(itemTex, 1, 18, Vector2.Zero);

            Position = position;
            _treasureIcon = treasureIcon;
            _treasureName = treasureName;
            _takenString = Global.TextManager.GetText((int)Global.HardCodedText.ITEM_ACQUISITION_MESSAGE, Global.CurrLang);
            _windowGrowthTimer = _windowGrowthRate;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawNineSlice(spriteBatch, gameTime, Position, _boxWidth, _boxHeight);
        }

        public void Update(GameTime gameTime)
        {
            if (Global.AnimationTimer.OneFrameElapsed())
            {
                if (_windowGrowthTimer <= 0)
                {
                    _windowGrowthTimer = _windowGrowthRate;

                    if (_boxWidth < 23) {
                        _boxWidth += 2;
                        Position += new Vector2(-World.CHIP_SIZE, 0);
                    } else
                    {
                        _doneDrawing = true;
                    }

                    if (_boxWidth > 10 && _boxHeight < 3)
                    {
                        _boxHeight += 2;
                        Position += new Vector2(0, -World.CHIP_SIZE);
                    }
                }
                else
                {
                    _windowGrowthTimer--;
                }
            }

            if (_doneDrawing)
            {
                if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_CONFIRM) || InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_CANCEL) || InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MAIN_WEAPON) || InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.SUB_WEAPON))
                {
                    Global.EntityManager.RemoveEntity(this);
                    Global.NineSliceBox = null;
                    Global.Main.SetState(Global.GameState.PLAYING);
                }
            }
        }

        public void DrawNineSlice(SpriteBatch spriteBatch, GameTime gameTime, Vector2 position, int columns, int rows)
        {
            var tSize = World.CHIP_SIZE;

            if (_visible)
            {
                // Top Left
                _boxSprites[(int)NSlice.TL].Draw(spriteBatch, new Vector2(position.X, position.Y));

                // Top Right
                _boxSprites[(int)NSlice.TR].Draw(spriteBatch, new Vector2(position.X + (columns * tSize), position.Y));

                // Bottom Left
                _boxSprites[(int)NSlice.BL].Draw(spriteBatch, new Vector2(position.X, position.Y + (rows * tSize)));

                // Bottom Right
                _boxSprites[(int)NSlice.BR].Draw(spriteBatch, new Vector2(position.X + (columns * tSize), position.Y + (rows * tSize)));

                // Edges
                for (var i = 1; i < rows; i++)
                {
                    // Left Edge
                    _boxSprites[(int)NSlice.L].Draw(spriteBatch, new Vector2(position.X, position.Y + (i * tSize)));

                    // Right Edge
                    _boxSprites[(int)NSlice.R].Draw(spriteBatch, new Vector2(position.X + (columns * tSize), position.Y + (i * tSize)));

                }
                for (var i = 1; i < columns; i++)
                {
                    // Top Edge
                    _boxSprites[(int)NSlice.TM].Draw(spriteBatch, new Vector2(position.X + (i * tSize), position.Y));

                    // Bottom Edge
                    _boxSprites[(int)NSlice.BM].Draw(spriteBatch, new Vector2(position.X + (i * tSize), position.Y + (rows * tSize)));
                }

                // Middle
                for (var i = 1; i < columns; i++)
                {
                    for (var j = 1; j < rows; j++)
                    {
                        _boxSprites[(int)NSlice.M].Draw(spriteBatch, new Vector2(position.X + (i * tSize), position.Y + (j * tSize)));
                    }
                }

                if (rows >= 3)
                {
                    if (_treasureIcon != null)
                        _treasureIcon.Draw(spriteBatch, new Vector2(Position.X + tSize, Position.Y + tSize));
                    string displayedTreasureName = _treasureName.Substring(0, Math.Clamp(_treasureName.Length, 1, columns - 8));
                    string displayedTakenString = _takenString.Substring(0, Math.Clamp(_takenString.Length, 1, columns - 7));

                    if (_doneDrawing)
                    {
                        displayedTreasureName = _treasureName;
                        displayedTakenString = _takenString;
                    }
                    Global.TextManager.DrawText(new Vector2(Position.X + (4 * tSize), Position.Y + (1 * tSize)), displayedTreasureName);
                    Global.TextManager.DrawText(new Vector2(Position.X + ((columns - 1) * tSize) - (displayedTakenString.Length * tSize), Position.Y + (2 * tSize)), displayedTakenString);
                    
                }
            }
        }
    }
}
