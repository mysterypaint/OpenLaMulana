using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using System.Collections.Generic;
using System.IO;

namespace OpenLaMulana
{
    public class AudioManager : IGameEntity
    {
        private MidiPlayer midiPlayer;
        private List<MidiFile> songs = new List<MidiFile>();
        private int currSongID = 0;//17;
        private string[] bgmFNames = new string[76];
        private bool guidanceGateFirstTime = true;

        public int DrawOrder => 0;

        public AudioManager()
        {
        }

        public void LoadContent(string musPath)
        {
            midiPlayer = new MidiPlayer(musPath + "SanbikiScc.sf2");

            for (var i = 0; i <= 75; i++)
            {
                string fName = "";
                if (i < 10)
                    fName += "0";
                fName += i.ToString();
                bgmFNames[i] = "m" + fName;
                songs.Add(new MidiFile(Path.Combine(musPath, bgmFNames[i] + ".mid")));
            }
        }

        public void UnloadContent()
        {
            midiPlayer.Dispose();
        }

        public void LoadSong(string fileName)
        {
        }

        internal void Update(GameTime gameTime)
        {
            if (midiPlayer.State == SoundState.Stopped && currSongID >= 0)
            {
                midiPlayer.Play(songs[currSongID], true);
            }
        }

        internal void StopMusic()
        {
            midiPlayer.Stop();
            currSongID = -1;
        }

        internal int IsPlaying()
        {
            return currSongID;
        }

        internal void PauseMusic()
        {
            midiPlayer.Pause();
        }

        internal void ResumeMusic()
        {
            midiPlayer.Resume();
        }
        internal void ChangeSongs(int musicNumber, bool isJukebox = false)
        {
            if (currSongID != musicNumber)
            {
                if (!isJukebox)
                {
                    if (musicNumber == 1)
                    {
                        if (currSongID == 39)
                            return;

                        if (guidanceGateFirstTime)
                        {
                            musicNumber = 39;
                            guidanceGateFirstTime = false;
                        }
                    }
                }
                midiPlayer.Stop();
                //midiPlayer.Dispose();
                currSongID = musicNumber;
                midiPlayer.Play(songs[currSongID]);
            }
        }

        void IGameEntity.Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }
    }
}