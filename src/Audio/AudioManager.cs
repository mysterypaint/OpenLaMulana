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
        private int _currSongID = -1;//17;
        private string[] _bgmFNames = new string[76];
        private bool _guidanceGateFirstTime = true;

        public int DrawOrder { get; set; } = 0;
        public Effect ActiveShader { get; set; } = null;

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
                _bgmFNames[i] = "m" + fName;
                songs.Add(new MidiFile(Path.Combine(musPath, _bgmFNames[i] + ".mid")));
            }

            string[] sfxList = {
                "se00",
                "se01",
                "se02",
                "se03",
                "se04",
                "se05",
                "se06",
                "se07",
                "se08",
                "se09",
                "se0A",
                "se0B",
                "se0C",
                "se0D",
                "se0E",
                "se0F",
                "se10",
                "se11",
                "se12",
                "se13",
                "se14",
                "se15",
                "se16",
                "se17",
                "se18",
                "se19",
                "se1A",
                "se1A_01",
                "se1B",
                "se1C",
                "se1D",
                "se1E",
                "se1F",
                "se20",
                "se21",
                "se22",
                "se23",
                "se24",
                "se25",
                "se26",
                "se27",
                "se28",
                "se29",
                "se2A",
                "se2B",
                "se2C",
                "se2D",
                "se2E",
                "se2F",
                "se30",
                "se31",
                "se32",
                "se33",
                "se34",
                "se35",
                "SE36",
                "SE37",
                "SE38",
                "SE39",
                "SE3A",
                "SE3B",
                "SE3C",
                "SE3D",
                "se3E",
                "se3F",
                "se40",
                "se41",
                "se42",
                "se43",
                "se44",
                "se45",
                "se46",
                "se47",
                "se48",
                "se49",
                "se4A",
                "se4B",
                "se4C",
                "se4D",
                "se4E",
                "se4F",
                "se50",
                "se51",
                "se52",
                "se53",
                "se54",
                "se55",
                "se56",
                "se57",
                "se58",
                "se59",
                "se5A",
                "se5B",
                "se5C"
            };
            
            for (SFX sfx = SFX.PAUSE; sfx < SFX.MAX; sfx++) {
                string fName = sfxList[(int)sfx];
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

        public void Update(GameTime gameTime)
        {
            if (midiPlayer.State == SoundState.Stopped && _currSongID >= 0)
            {
                midiPlayer.Play(songs[_currSongID], true);
                Debug.WriteLine(midiPlayer.GetMasterVolume());
            }
        }

        internal void StopMusic()
        {
            midiPlayer.Stop();
            _currSongID = -1;
        }

        internal int IsPlaying()
        {
            return _currSongID;
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
            if (_currSongID != musicNumber)
            {
                if (!isJukebox)
                {
                    if (musicNumber == 1)
                    {
                        if (_currSongID == 39)
                            return;

                        if (_guidanceGateFirstTime)
                        {
                            musicNumber = 39;
                            _guidanceGateFirstTime = false;
                        }
                    }
                }
                midiPlayer.Stop();
                //midiPlayer.Dispose();
                _currSongID = musicNumber;
                midiPlayer.Play(songs[_currSongID]);
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