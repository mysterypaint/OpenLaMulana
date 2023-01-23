using OpenLaMulana.Entities;
using System;

namespace OpenLaMulana
{
    public class Chip
    {
        public enum TileTypes : int
        {
            BLANK,
            SOLID
        };


        public int TileID = -1;
        public float CurrFrame = 0;
        public bool IsAnime = false;
        public int AnimeSpeed = 0;
        public World.ChipTypes SpecialChipBehavior = World.ChipTypes.UNDEFINED;
        public bool IsOverlay { get; set; } = false;
        private int _animationTimer = 0;
        private int[] _animeFrames = null;


        public Chip(short tileID = 0, int[] animeTileInfo = null)
        {
            this.TileID = tileID;
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
                AnimeSpeed = animeTileInfo[1];
                IsAnime = true;
            }

            _animationTimer = AnimeSpeed;
        }

        public int[] GetAnimeFramesAsRawData()
        {
            if (!IsAnime)
                return null;
            int[] rawAnimeFrames = new int[2 + _animeFrames.Length - 1];
            rawAnimeFrames[0] = _animeFrames[0] - World.ANIME_TILES_BEGIN;
            rawAnimeFrames[1] = AnimeSpeed;
            Array.Copy(_animeFrames, 1, rawAnimeFrames, 2, _animeFrames.Length - 1);
            return rawAnimeFrames;
        }

        internal void StepFrame()
        {
            _animationTimer--;

            if (_animationTimer < 0) {
                CurrFrame++;
                if (CurrFrame >= _animeFrames.Length)
                    CurrFrame = 0;

                _animationTimer = AnimeSpeed;
            }
        }

        internal void SetAnimeFrames(int[] animeFrames)
        {
            if (animeFrames != null) {
                _animeFrames = animeFrames;
            }
        }

        internal void CloneTile(Chip modelChip)
        {
            this.TileID = modelChip.TileID;
            this.AnimeSpeed = modelChip.AnimeSpeed;
            this.SetAnimeFrames(modelChip.GetAnimeFrames());
            this.IsAnime = modelChip.IsAnime;
            this.IsOverlay = modelChip.IsOverlay;
            this.SpecialChipBehavior = modelChip.SpecialChipBehavior;
        }

        internal int[] GetAnimeFrames()
        {
            return _animeFrames;
        }
    }
}