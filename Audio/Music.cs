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

        public static object MusicTask { get;  }

        //public static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        //{ // converts 32 bit float samples to 16 bit pcm. I think :P
        //    // https://stackoverflow.com/questions/31957211/how-to-convert-an-array-of-int16-sound-samples-to-a-byte-array-to-use-in-monogam/42151979#42151979
        //    byte[] pcm = new byte[samplesCount * 2];
        //    int sampleIndex = 0,
        //        pcmIndex = 0;

        //    while (sampleIndex < samplesCount)
        //    {
        //        short outsample = (short)(samples[sampleIndex] * short.MaxValue);
        //        pcm[pcmIndex] = (byte)(outsample & 0xff);
        //        pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

        //        sampleIndex++;
        //        pcmIndex += 2;
        //    }

        //    return pcm;
        //}
        private static bool musicplaying = false;

        private static int lastplayed = -1;

        public static void PlayStop(ushort? index = null, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (!musicplaying || lastplayed != currSongID)
            {
                Play(index: index, volume: volume, pitch: pitch, pan: pan);
            }
            else
            {
                Stop();
            }
        }

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

            // Use FluidMIDI if we're on a not-Windows OS or 64-bit Windows. Otherwise, use DirectMidi

#if _X64 || !_WINDOWS
            if (fluid_Midi == null)
                fluid_Midi = new Audio.Midi.Fluid();
            fluid_Midi.ReadSegmentFileManually(filename);
            fluid_Midi.Play();
#else
                    
                                    if (dm_Midi == null)
                                        dm_Midi = new Audio.Midi.DirectMedia();
                                    dm_Midi.Play(filename,loop);
                    
#endif

            musicplaying = true;
            lastplayed = currSongID;
        }
        
        public static void KillAudio()
        {
            dm_Midi?.Dispose();
            fluid_Midi?.Dispose();
        }

        public static void Stop()
        {
            musicplaying = false;
            cancelTokenSource?.Cancel();
#if !_X64
            if (dm_Midi != null)
                dm_Midi.Stop();
#else
            fluid_Midi?.Stop();
#endif
        }

        private static Audio.Midi.DirectMedia dm_Midi;
        private static Audio.Midi.Fluid fluid_Midi;
        private static CancellationTokenSource cancelTokenSource;
        private static CancellationToken cancelToken;
        private static int currSongID;
    }
}