using OpenLaMulana.Entities;
using System;

namespace OpenLaMulana
{
    public class Chip
    {
        public enum TileTypes
        {
            BLANK,
            SOLID
        };


        public int tileID = -1;
        public float currFrame = 0;
        public bool isAnime = false;
        public int animeSpeed = 0;
        private int[] _animeFrames = null;

        public Chip(short tileID = 0, int[] animeTileInfo = null)
        {
            this.tileID = tileID;
            //Array.Copy(a, 1, b, 0, 3);
            //animatedTileInfo;

            if (animeTileInfo != null)
            {
                // Example animation array: {0, 1, 26, 28, 30}
                // 0 -> Animation index, which is also the first frame of the animation TileAt(1160 + 0)
                // 1 -> Animation Speed
                // 26 -> Animation Frame #2: TileAt(26)
                // 28 -> Animation Frame #3: TileAt(28)
                // 30 -> Animation Frame #4: TileAt(30)
                // Can add (or remove) more/less animation frames to this array

                _animeFrames = new int[animeTileInfo.Length - 1];
                Array.Copy(animeTileInfo, 2, _animeFrames, 1, animeTileInfo.Length - 2);
                _animeFrames[0] = animeTileInfo[0] + World.ANIME_TILES_BEGIN;
                animeSpeed = animeTileInfo[1];
                isAnime = true;
            }
        }

        public int[] GetAnimeFrames()
        {
            return _animeFrames;
        }
    }
}