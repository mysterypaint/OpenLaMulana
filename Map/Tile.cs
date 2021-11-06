using System;

namespace OpenLaMulana
{
    public class Tile
    {
        public int _tileID = -1;
        public int _currFrame = 0;
        public bool isAnime = false;
        public int animeSpeed = 0;
        private int[] _animeFrames = null;

        public Tile(short tileID, int[] animeTileInfo)
        {
            this._tileID = tileID;
            //Array.Copy(a, 1, b, 0, 3);
            //animatedTileInfo;

            if (animeTileInfo != null)
            {
                this._animeFrames = new int[animeTileInfo.Length - 1];
                Array.Copy(animeTileInfo, 2, _animeFrames, 1, animeTileInfo.Length - 2);
                _animeFrames[0] = animeTileInfo[0] + Field.ANIME_TILES_BEGIN;
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