using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;

namespace OpenLaMulana
{
    public class AudioManager : IGameEntity
    {
        private MidiPlayer _midiPlayer;
        private List<MidiFile> songs = new List<MidiFile>();
        private int _currSongID = -1;//17;
        private string[] _bgmFNames = new string[76];
        public bool LockTo30FPS { get; set; } = false;

        public int Depth { get; set; } = (int)Global.DrawOrder.Abstract;
        public Effect ActiveShader { get; set; } = null;

        private float _userMasterVolScale { get; set; } = 0.8f;
        private float _userBGMVolScale { get; set; } = 0.8f;
        private float _userSFXVolScale { get; set; } = 0.8f;

        private float _internalBGMVolScale = 0.55f;//0.6f;
        private float _internalSFXVolScale = 1.0f;

        private static Dictionary<SFX, SoundEffect> _sfxBank = new Dictionary<SFX, SoundEffect>();

        private List<SoundEffectInstance> _activeSoundEffects = new List<SoundEffectInstance>();
        private bool _noAudioHardware = false;

        public AudioManager()
        {
        }

        public void LoadContent(string musPath, ContentManager content)
        {
            int sampleRate = 44100;// 44100;

            SynthesizerSettings settings = new SynthesizerSettings(sampleRate);
            _midiPlayer = new MidiPlayer(musPath + "SanbikiScc.sf2", settings);
            _midiPlayer.SetMasterVolume(_userMasterVolScale * _userBGMVolScale * _internalBGMVolScale);
            SoundEffect.MasterVolume = _userMasterVolScale * _userSFXVolScale * _internalSFXVolScale;

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

            try
            {
                for (SFX sfx = SFX.PAUSE; sfx < SFX.MAX; sfx++)
                {
                    string fName = sfxList[(int)sfx];
                    _sfxBank[sfx] = content.Load<SoundEffect>(Path.Combine("sound", fName));
                    _activeSoundEffects.Add(_sfxBank[sfx].CreateInstance());
                }
            }
            catch (NoAudioHardwareException)
            {
                _noAudioHardware = true;
            }

        }

        public void UnloadContent()
        {
            if (_noAudioHardware)
                return;
            _midiPlayer.Dispose();
            foreach (SoundEffectInstance sfx in _activeSoundEffects)
            {
                sfx.Dispose();
            }
            _activeSoundEffects.Clear();

            for (SFX sfx = SFX.PAUSE; sfx < SFX.MAX; sfx++)
            {
                _sfxBank[sfx].Dispose();
            }
        }

        public void LoadSong(string fileName)
        {
        }

        public void Update(GameTime gameTime)
        {
            if (_noAudioHardware)
                return;
            if (_midiPlayer.State == SoundState.Stopped && _currSongID >= 0)
            {
                _midiPlayer.Play(songs[_currSongID], true);
                Debug.WriteLine(_midiPlayer.GetMasterVolume());
            }
        }

        public void ToggleMIDIChannel(int channel)
        {
            _midiPlayer.ToggleChannel(channel - 1);
        }

        public float GetChannelVolume(int channelID)
        {
            return _midiPlayer.GetChannelVolume(channelID - 1);
        }

        internal void StopMusic()
        {
            _midiPlayer.Stop();
            _currSongID = -1;
        }

        internal int IsPlaying()
        {
            return _currSongID;
        }

        internal void PauseMusic()
        {
            _midiPlayer.Pause();
        }

        internal void ResumeMusic()
        {
            _midiPlayer.Resume();
        }

        internal void ChangeSongs(int musicNumber, bool isJukebox = false)
        {
            if (_currSongID != musicNumber)
            {
                if (!isJukebox)
                {
                    switch (musicNumber)
                    {
                        case 0:
                            if (_currSongID == 17)
                                return;

                            if (Global.GameFlags.InGameFlags[(int)GameFlags.Flags.GUARDIAN_DEFEATED_ALL])
                            {
                                musicNumber = 17;
                            }
                            break;
                        case 1:
                            if (_currSongID == 39)
                                return;
                            
                            if (!Global.GameFlags.InGameFlags[(int)GameFlags.Flags.ENTERED_LAMULANA_FOR_THE_FIRST_TIME])
                            {
                                musicNumber = 39;
                            }
                            break;
                    }
                }
                _midiPlayer.Stop();
                //midiPlayer.Dispose();
                _currSongID = musicNumber;
                _midiPlayer.Play(songs[_currSongID]);
            }
        }

        public float GetUserMasterVolume()
        {
            return _userMasterVolScale;
        }
        public float GetUserBGMVolume()
        {
            return _userBGMVolScale;
        }
        public float GetUserSFXVolume()
        {
            return _userSFXVolScale;
        }

        public void SetMasterVolume(float newVal)
        {
            if (_noAudioHardware)
                return;
            _userMasterVolScale = newVal;
            float preSet = (float)Math.Round((_userMasterVolScale * _userBGMVolScale * _internalBGMVolScale) * 100) / 100;
            _midiPlayer.SetMasterVolume(preSet);
            //SoundEffect.MasterVolume = _userMasterVolScale * _userSFXVolScale * _internalSFXVolScale;

            // Hacky solution, because SoundEffect.Master volume is shared with the BGM synthesizer...
            int i = 0;
            for (SFX sfx = SFX.PAUSE; sfx < SFX.MAX; sfx++)
            {
                _activeSoundEffects[i].Volume = _userMasterVolScale * _userSFXVolScale * _internalSFXVolScale;
                i++;
            }


        }

        public void SetBGMVolume(float newVal)
        {
            if (_noAudioHardware)
                return;
            _userBGMVolScale = newVal;
            float preSet = (float)Math.Round((_userMasterVolScale * _userBGMVolScale * _internalBGMVolScale) * 100) / 100;
            _midiPlayer.SetMasterVolume(preSet);
        }

        public void SetSFXVolume(float newVal)
        {
            if (_noAudioHardware)
                return;
            _userSFXVolScale = newVal;

            // Hacky solution, because SoundEffect.Master volume is shared with the BGM synthesizer...
            int i = 0;
            for (SFX sfx = SFX.PAUSE; sfx < SFX.MAX; sfx++)
            {
                _activeSoundEffects[i].Volume = _userMasterVolScale * _userSFXVolScale * _internalSFXVolScale;
                i++;
            }


            //SoundEffect.MasterVolume = _userMasterVolScale * _userSFXVolScale * _internalSFXVolScale; // SoundEffect.MasterVolume and SoundEffectInstance.Volume both range from 0.0f to 1.0f
        }

        internal float GetMasterBGMVolume()
        {
            return _midiPlayer.GetMasterVolume();
        }

        internal float GetMasterSFXVolume()
        {
            return SoundEffect.MasterVolume;
        }

        public UInt32 GetEnabledChannels()
        {
            return _midiPlayer.GetEnabledChannels();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        internal void PlaySFX(SFX sfxId)
        {
            if (_noAudioHardware)
                return;
            _activeSoundEffects[(int)sfxId].Stop();
            _activeSoundEffects[(int)sfxId].Play();
        }

        internal void SetEnabledChannels(UInt32 value)
        {
            _midiPlayer.SetEnabledChannels(value);
        }
    }
}