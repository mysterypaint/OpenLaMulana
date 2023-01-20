using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.WorldEntities.NPCRoom;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class InvisibleCollider : InteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        private int _flagToSet = -1;
        private int _deletionJudgement = -1;

        /// <summary>
        /// An object that turns on the flag by contacting the main character.
        /// In OP1 and 2, the size of the event is determined by the map chip size.
        /// Turns on the flag specified in OP3 by contacting the main character.
        /// Specifying 0 in OP4 turns off the flag specified in OP3.
        /// The range of the event is specified in map chip units, but the main character has collision detection with walls and events only in dot units,
        /// the size of the legs open when standing still.
        /// 
        /// For this reason, the hero can sink into the wall a little. If it deviates to this point, the contact judgment with the event will also monitor the wall side.
        /// When using this event in a narrow space surrounded by walls, it is a good idea to set it up one character wide on the left and right.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tilesWide"></param>
        /// <param name="tilesHigh"></param>
        /// <param name="flagToSet"></param>
        /// <param name="deletionJudgement"></param> // (-1 by default; 0 turns off the OP3 flag)
        /// <param name="spawnIsGlobal"></param>
        /// <param name="destView"></param>
        /// <param name="startFlags"></param>
        public InvisibleCollider(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            //_tex = Global.TextureManager.MakeTexture(16, 8, new Vector4(0, 255, 0, 255));
            //_sprIndex = new Sprite(_tex, 0, 8, 16, 8);
            
            HitboxWidth = op1 * World.CHIP_SIZE;
            HitboxHeight = op2 * World.CHIP_SIZE;
            _flagToSet = op3;
            _deletionJudgement = op4;
            Depth = (int)Global.DrawOrder.AboveTilesetGraphicDisplay;
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                    Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y + Main.HUD_HEIGHT, HitboxWidth, HitboxHeight);//(int)(0.5f * (HitboxWidth / World.CHIP_SIZE)), (int)(0.5f * (HitboxWidth / World.CHIP_SIZE)));
                    HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(255, 134, 0, 40));
                    break;
            }
            //Rectangle offBox = new Rectangle(BBox.X, BBox.Y + World.HUD_HEIGHT, BBox.Width, BBox.Height);
            //HelperFunctions.DrawRectangle(spriteBatch, offBox, Color.Green);
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = WEStates.IDLE;
                    }
                    break;
                case WEStates.IDLE:
                    if (CollidesWithPlayer())
                    {
                        if (_flagToSet > -1)
                        {
                            Global.GameFlags.InGameFlags[_flagToSet] = true;
                        }
                    }
                    break;
            }
        }
    }
}