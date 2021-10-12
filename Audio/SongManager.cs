using System;
using System.Diagnostics;
using ChaiFoxes.FMODAudio;

namespace OpenLaMulana
{
    public class SongManager
    {
        public SongManager()
        {
        }

        public void InitPlayer()
        {
            FMODManager.Init(FMODMode.CoreAndStudio, "Content");
        }

        public void LoadSong(string fileName)
        {

            var sound = CoreSystem.LoadStreamedSound(fileName);
            var channel = sound.Play();
            channel.Looping = true;
        }
    }
}