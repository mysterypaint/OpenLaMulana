using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OpenLaMulana.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenLaMulana
{
    public class AudioManager
    {
        private MidiPlayer midiPlayer;
        private List<MidiFile> songs = new List<MidiFile>();
        private int currSongID = 17;
        private string[] bgmFNames = new string[76];
        private bool guidanceGateFirstTime = true;

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
                songs.Add(new MidiFile(Path.Combine(musPath, "m" + fName + ".mid")));
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

        internal void PauseMusic()
        {
            midiPlayer.Pause();
        }

        internal void ResumeMusic()
        {
            midiPlayer.Resume();
        }
        internal void ChangeSongs(int musicNumber)
        {
            if (currSongID != musicNumber)
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
                midiPlayer.Stop();
                //midiPlayer.Dispose();
                currSongID = musicNumber;
                midiPlayer.Play(songs[currSongID], true);
            }
        }
    }
}