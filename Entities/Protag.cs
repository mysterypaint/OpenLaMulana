using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using static OpenLaMulana.Tile;

namespace OpenLaMulana.Entities
{

    public class Protag : IGameEntity, ICollidable
    {
        enum Facing
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        };

        private Sprite _idleSprite;

        private SoundEffect _jumpSound;

        public PlayerState State { get; private set; }
        PlayerState prev_state = PlayerState.IDLE;

        private World _world;

        public Vector2 Position { get; set; }
        public int moveX = 0;
        public int moveY = 0;
        public float hsp = 0;
        public float vsp = 0;
        private bool hasBoots = true;
        private bool hasFeather = true;
        float move_speed = 0.8f;
        int jumps = 0;
        int jumps_max = 2;
        float grav = 0.34f;
        int jump_speed = 5;
        double grav_max = 0.8;
        bool grounded = false;
        static int jump_timer_max = 4;
        int jump_timer = jump_timer_max;
        static int second_jump_timer_max = 10;
        int second_jump_timer = second_jump_timer_max;
        int fall_timer = 0;
        readonly int fall_timer_max = 40;
        readonly int whip_cooldown_timer_max = 29;
        int whip_cooldown_timer = 0;
        float img_index = 0;
        float img_index_offset = 0;
        int img_anim_max = 1;
        float img_speed = 0;
        int state = 0;
        int p_facing_x = 1;
        bool straight_fall = true;
        int move_x = 0;

        private Facing facingX = Facing.RIGHT;
        private Facing facingY = Facing.DOWN;

        private InputController _inputController = null;

        public short bBoxOriginX { get; set; }
        public short bBoxOriginY { get; set; }

        public int DrawOrder { get; set; }

        public Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - bBoxOriginX),
                    (int)Math.Round(Position.Y - bBoxOriginY),
                    10,
                    12
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public Protag(Texture2D spriteSheet, World world, Vector2 position, SoundEffect jumpSound)
        {
            _world = world;
            Position = position;
            State = PlayerState.IDLE;


            bBoxOriginX = 5;
            bBoxOriginY = 12;

            _jumpSound = jumpSound;

            _idleSprite = new Sprite(spriteSheet, 0, 0, 16, 16, 8, 16);

            if (!hasFeather)
                second_jump_timer_max = -1;
        }

        public void Initialize()
        {
            State = PlayerState.IDLE;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _idleSprite.Draw(spriteBatch, Position);

            RectangleSprite.DrawRectangle(spriteBatch, CollisionBox, Color.Red, 1);
        }

        public void Update(GameTime gameTime)
        {
            var posX = Position.X;
            var posY = Position.Y;

            if (vsp > 0)
            {
                State = PlayerState.FALLING;
                //default_animation()
            }
            if (!hasBoots)
                move_speed = 0.8f;
            else
                move_speed = 1.6f;
            if (State != PlayerState.WHIPPING && whip_cooldown_timer <= ((whip_cooldown_timer_max - 1) / 2) && (!straight_fall))
            {
                if (grounded || whip_cooldown_timer > 0)
                    move_x = _inputController.dirMoveX;


                if (PlaceMeeting(posX, (posY + 1), TileTypes.SOLID))
                {
                    if (move_x != 0)
                    {
                        if (State != PlayerState.WALKING)
                        {
                            State = PlayerState.WALKING;
                            img_anim_max = 2;
                            img_index = 0;
                            img_index_offset = 0;
                            if (!hasBoots)
                                img_speed = 0.12f;
                            else
                                img_speed = 0.18f;
                        }
                    }
                    else
                    {
                        State = PlayerState.IDLE;
                        //default_animation();
                    }
                }
            }
            else if (grounded)
                move_x = 0;
            if (prev_state == PlayerState.WALKING && State == PlayerState.FALLING)
            {
                straight_fall = true;
                hsp = 0;
                move_x = 0;
            }
            if (move_x == 0 && grounded)
                hsp *= 0.4f;

            if (PlaceMeeting(posX, (posY + 1), TileTypes.SOLID))
            {
                if (straight_fall)
                {
                    straight_fall = false;
                    //audio_play_sound(sfxGrounded, 0, false)
                }
                jumps = jumps_max;
                second_jump_timer = second_jump_timer_max;
                if (!_inputController.keyJumpHeld)
                    jump_timer = jump_timer_max;
                if (State != PlayerState.WHIPPING && State != PlayerState.WALKING)
                {
                    State = PlayerState.IDLE;
                    //default_animation()
                }
            }
            hsp = (move_x * move_speed);
            if (whip_cooldown_timer <= 0 && _inputController.keyWhipPressed)
            {
                if (p_facing_x == 1)
                {
                    //var _whip = instance_create_depth((posX - 16), posY, -999, PlayerWhip)
                    //_whip.image_index = 33;
                }
                else
                {
                    //_whip = instance_create_depth((posX + 13), (posY - 5), -999, PlayerWhip)
                    //_whip.image_index = 35;
                }
                //audio_stop_sound(sfxPlayerWhip);
                //audio_play_sound(sfxPlayerWhip, 0, false);
                //whip_cooldown_timer = whip_cooldown_timer_max;
            }
            if (whip_cooldown_timer > 0)
            {
                State = PlayerState.WHIPPING;
                //default_animation();
                whip_cooldown_timer--;
            }
            if (whip_cooldown_timer == ((whip_cooldown_timer_max - 1) / 2))
            {
                //audio_stop_sound(sfxPlayerWhip)
                //audio_play_sound(sfxPlayerWhip2, 0, false)
            }
            if (fall_timer > 0)
                fall_timer--;
            if (_inputController.keyJumpHeld)
                jump_timer--;
            else if (vsp < 0)
                vsp = Math.Max(vsp, 0.08f);
            if (jumps == 1)
            {
                if (second_jump_timer > 0)
                    second_jump_timer--;
            }
            if (jumps > 1 && _inputController.keyJumpHeld && jump_timer == 0 && vsp <= 0)
            {
                vsp = (-jump_speed);
                fall_timer = fall_timer_max;
                if (jumps == jumps_max)
                    hsp *= 2;
                State = PlayerState.JUMPING;
                //default_animation()
                jumps--;
                if (jumps == 1)
                {
                    //audio_play_sound(sfxJump, 0, false)
                }
                grounded = false;
            }
            else if (second_jump_timer == 0 && jumps == 1 && _inputController.keyJumpPressed && (vsp <= 0 || (State == PlayerState.FALLING && fall_timer > 0)))
            {
                vsp = (-jump_speed);
                //audio_play_sound(sfxJump, 0, false);
                State = PlayerState.JUMPING;
                //default_animation()
                jumps--;
            }
            if ((vsp + grav) < 6)
                vsp += grav;
            else
                vsp = 8f;
            if (PlaceMeeting((posX + hsp), posY, TileTypes.SOLID))
            {
                while (!PlaceMeeting((posX + Math.Sign(hsp)), posY, TileTypes.SOLID))
                    posX += Math.Sign(hsp);
                hsp = 0;
            }
            posX += hsp;
            if (PlaceMeeting(posX, (posY + vsp), TileTypes.SOLID))
            {
                while (!PlaceMeeting(posX, (posY + Math.Sign(vsp)), TileTypes.SOLID))
                    posY += Math.Sign(vsp);
                vsp = 0;
                if (!grounded)
                {
                    //audio_stop_sound(sfxGrounded)
                    //audio_play_sound(sfxGrounded, 0, false)
                    hsp = 0;
                }
                grounded = true;
            }
            posY += vsp;
            if (move_x == 1)
                p_facing_x = 1;
            else if (move_x == -1)
                p_facing_x = 0;
            switch (State)
            {
                case PlayerState.IDLE:
                default:
                    if (p_facing_x == 1)
                    {
                        img_index_offset = 0;
                        if (_inputController.keyJumpHeld && jump_timer > 0)
                            img_index_offset = 6;
                    }
                    else
                    {
                        img_index_offset = 2;
                        if (_inputController.keyJumpHeld && jump_timer > 0)
                            img_index_offset = 8;
                    }
                    break;
                case PlayerState.WALKING:
                    if (p_facing_x == 1)
                    {
                        img_index_offset %= 2;
                        if (_inputController.keyJumpHeld && jump_timer > 0)
                            img_index_offset = 6;
                    }
                    else
                    {
                        img_index_offset = (2 + (img_index_offset % 2));
                        if (_inputController.keyJumpHeld && jump_timer > 0)
                            img_index_offset = 8;
                    }
                    break;
                case PlayerState.JUMPING:
                    if (p_facing_x == 1)
                        img_index_offset = 6;
                    else
                        img_index_offset = 8;
                    break;
                case PlayerState.FALLING:
                    if (p_facing_x == 1)
                        img_index_offset = 7;
                    else
                        img_index_offset = 9;
                    break;
                case PlayerState.WHIPPING:
                    var xoff = -5;
                    var yoff = -11;
                    if (whip_cooldown_timer > (whip_cooldown_timer_max / 2))
                    {
                        if (p_facing_x == 1)
                        {
                            img_index_offset = 16;
                            //PlayerWhip.image_index = 32;
                            //PlayerWhip.posX = (posX - 3 + xoff);
                            //PlayerWhip.posY = (posY - 5 + yoff);
                        }
                        else
                        {
                            //PlayerWhip.image_index = 34;
                            //PlayerWhip.posX = (posX + 13 + xoff);
                            //PlayerWhip.posY = (posY - 5 + yoff);
                            img_index_offset = 18;
                        }
                    }
                    else if (whip_cooldown_timer > 0)
                    {
                        if (p_facing_x == 1)
                        {
                            img_index_offset = 17;
                            //PlayerWhip.image_index = 33;
                            //PlayerWhip.posX = (posX + 21 + xoff);
                            //PlayerWhip.posY = (posY + 11 + yoff);
                        }
                        else
                        {
                            ;                           //PlayerWhip.image_index = 35;
                                                        //PlayerWhip.posX = (posX - 11 + xoff);
                                                        //PlayerWhip.posY = (posY + 11 + yoff);
                            img_index_offset = 19;
                        }
                    }
                    else
                    {
                        State = PlayerState.IDLE;
                        //default_animation();
                        //instance_destroy(PlayerWhip);
                    }
                    break;
            }

            // Update the actual position
            Position = new Vector2(posX, posY);

            img_index += img_speed;
            prev_state = State;
        }

        private bool PlaceMeeting(float xCheck, float yCheck, TileTypes tileType)
        {
            if (_world == null)
                return false;
            var currField = _world.GetField(_world.currField);
            var rx = _world.currRoomX;
            var ry = _world.currRoomY;
            var currRoom = currField.GetMapData()[rx, ry];

            var rtx = Math.Floor(xCheck / World.tileWidth);
            var rty = Math.Floor(yCheck / World.tileHeight);

            var chipline = currField.GetChipline();

            if (!(rtx >= 0 && rtx <= Field.RoomWidth - 1 &&
                rty >= 0 && rty <= Field.RoomHeight - 1))
                return true;

            var checkingTileID = currRoom.Tiles[(int)rtx, (int)rty]._tileID;

            if (checkingTileID >= chipline[0] && checkingTileID < chipline[1])
                return true;
            else
                return false;
        }

        public void SetInputController(InputController inputController)
        {
            _inputController = inputController;
        }

        internal string PrintState()
        {
            string str = "Undefined";
            switch (State)
            {
                case PlayerState.IDLE:
                    return "Idle";
                case PlayerState.CUTSCENE:
                    return "Cutscene";
                case PlayerState.FALLING:
                    return "Falling";
                case PlayerState.JUMPING:
                    return "Jumping";
                case PlayerState.MAX:
                    return "Max";
                case PlayerState.PAUSED:
                    return "Paused";
                case PlayerState.WALKING:
                    return "Walking";
                case PlayerState.WHIPPING:
                    return "Whipping";
            }
            return str;
        }
    }
}
