using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GraphicDisplay : InteractableWorldEntity
    {
        private int _startFlag = -1;

        public GraphicDisplay(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            Vector2 _texCoords = HelperFunctions.GetOPCoords(op1);
            Vector2 _spriteWidthHeight = HelperFunctions.GetOPCoords(op2);

            _startFlag = op3;
            int _texturePage = op4;

            switch (_texturePage)
            {
                default:
                case 0:
                    _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                    Depth = (int)Global.DrawOrder.AboveTilesetGraphicDisplay;
                    break;
                case 2:
                    _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                    Depth = (int)Global.DrawOrder.AboveEntitiesGraphicDisplay;
                    break;
                case 3:
                    _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                    Depth = (int)Global.DrawOrder.Foreground;
                    break;
                case 4:
                    _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                    Depth = (int)Global.DrawOrder.Overlay;
                    break;
            }

            _sprIndex = new Sprite(_tex, _texCoords, _spriteWidthHeight);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}