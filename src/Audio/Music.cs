using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenLaMulana
{
    // VERY massive thank you to Sebanisu (github.com/Sebanisu) from the OpenVIII project for not only this class, but also providing the most documentation on the .sgt music format I've seen anywhere online! :D
    public static class Music
    {
        public static void Init()
        {
            //Debug.WriteLine($"{nameof(Music)} :: {nameof(Init)}");
            
            //var dmusic_pt = "Content/Music/";

        }

        public static bool Playing => musicplaying;

        private static bool musicplaying = false;

        private static int lastplayed = -1;

        private static int currSongID;
        public static void Play(ushort? index = null, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f, bool loop = true)
        {
            if (musicplaying && lastplayed == currSongID) return;

            string fName = "m";

            if (index <= 9)
                fName += "0";

            fName += index.ToString();
            fName += ".sgt";

            var filename = Path.Combine("Content/music/", fName);

            Debug.WriteLine("Attempting to play: " + filename);

            Stop();


            musicplaying = true;
            lastplayed = currSongID;
        }
        
        public static void KillAudio()
        {

        }

        public static void Stop()
        {

        }
    }
}