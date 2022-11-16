using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public float AudioManBGMVolScale { get; private set; } = 0.8f;
        public float AudioManSFXVolScale { get; private set; } = 1.0f;

        private static Dictionary<SFX, SoundEffect> _sfxBank = new Dictionary<SFX, SoundEffect>();

        public AudioManager()
        {
        }

        public void LoadContent(string musPath, ContentManager content)
        {
            int sampleRate = 44100;

            SynthesizerSettings settings = new SynthesizerSettings(sampleRate);
            midiPlayer = new MidiPlayer(musPath + "SanbikiScc.sf2", settings);
            midiPlayer.SetMasterVolume(AudioManBGMVolScale * 1.0f);

            for (var i = 0; i <= 75; i++)
            {
                string fName = "";
                if (i < 10)
                    fName += "0";
                fName += i.ToString();
                bgmFNames[i] = "m" + fName;
                songs.Add(new MidiFile(Path.Combine(musPath, bgmFNames[i] + ".mid")));
            }

            var sfxList = Enum.GetValues(typeof(SFX));

            foreach(SFX sfx in sfxList)
            {
                if (sfx == SFX.MAX)
                    break;
                string fName = "se";
                int sfxID = (int)sfx;
                if (sfxID < 0xA)
                    fName += 0;
                fName += sfxID.ToString();
                _sfxBank[sfx] = content.Load<SoundEffect>(Path.Combine("sound", fName));
            }

            SoundEffect.MasterVolume = 1;
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
                Debug.WriteLine(midiPlayer.GetMasterVolume());
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

        internal void SetMasterBGMVolume(float newVal)
        {
            midiPlayer.SetMasterVolume(AudioManBGMVolScale * newVal);
        }

        internal float GetMasterBGMVolume()
        {
            return midiPlayer.GetMasterVolume();
        }

        internal void SetMasterSFXVolume(float newVal)
        {
            SoundEffect.MasterVolume = AudioManSFXVolScale * newVal; // SoundEffect.MasterVolume and SoundEffectInstance.Volume both range from 0.0f to 1.0f
        }

        internal float GetMasterSFXVolume()
        {
            return SoundEffect.MasterVolume;
        }

        void IGameEntity.Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        internal void PlaySFX(SFX sfxId)
        {
            SoundEffectInstance inst = _sfxBank[sfxId].CreateInstance();

            inst.Stop();
            inst.Play();
        }
    }
}