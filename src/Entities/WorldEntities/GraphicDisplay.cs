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
        private int _checkFlag = -1;
        private bool _notSolidWhenOP3IsOn = false;
        private bool _isSolid = false;
        private Sprite _displayedGraphic;

        public GraphicDisplay(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            Vector2 _texCoords = HelperFunctions.GetOPCoords(op1);
            Vector2 _spriteWidthHeight = HelperFunctions.GetOPCoords(op2);

            _checkFlag = op3;

            bool _useEventTexture = false;
            if (op4 > 2)
            {
                _notSolidWhenOP3IsOn = HelperFunctions.GetBit((byte)op4, 3);
                _isSolid = HelperFunctions.GetBit((byte)op4, 2);
                _useEventTexture = HelperFunctions.GetBit((byte)op4, 1);
            } else if (op4 > 1)
            {
                _useEventTexture = HelperFunctions.GetBit((byte)op4, 1);
            } else if (op4 == 1)
            {
                _useEventTexture = true;
            }

            if (_useEventTexture)
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                Depth = (int)Global.DrawOrder.AboveEntitiesGraphicDisplay;
            } else
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                Depth = (int)Global.DrawOrder.AboveEntitiesGraphicDisplay;
            }

            _displayedGraphic = new Sprite(_tex, _texCoords, _spriteWidthHeight);

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
            {
                State = Global.WEStates.IDLE;
                _sprIndex = _displayedGraphic;
            }
            else
            {
                State = Global.WEStates.UNSPAWNED;
                _sprIndex = null;
            }
            /*
        switch (_texturePage)
        {
            default:
            case 0:
                break;
            case 1:
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
            */
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
            /*
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    break;
                case Global.WEStates.IDLE:
                    break;
            }*/
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = _displayedGraphic;
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[_checkFlag])
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = null;
                    }
                    break;
            }
        }
    }
}