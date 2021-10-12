using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
            /*
            var sound = CoreSystem.LoadStreamedSound(fileName);
            var channel = sound.Play();
            channel.Looping = true;
            */

            //FMOD.CREATESOUNDEXINFO

            /*
            var sound = CoreSystem.Native.createSound("sdflk.mid", FMODMode.Core, FMOD.Sound.);
            var channel = sound.Play();
            channel.Looping = true;
            */

            /*
            IntPtr pathInt = "gm.dls");
            info.dlsname = pathInt;
            info.cbsize = Marshal.SizeOf(info);
            FMOD.Sound sound;
            lowSys.createStream(Utils.dataPath + "errors.mid", FMOD.MODE.DEFAULT, ref info, out sound);
            FMOD.Channel channel;
            lowSys.playSound(sound, null, false, out channel);
            Marshal.FreeHGlobal(pathInt);
            */
            var info = new FMOD.CREATESOUNDEXINFO();

            string dataPath = Path.GetFullPath("Content/music/");

            IntPtr pathInt = Marshal.StringToHGlobalAnsi(dataPath + "gm.dls");//SanbikiScc
            info.dlsname = pathInt;
            info.cbsize = Marshal.SizeOf(info);
            
            info.suggestedsoundtype = FMOD.SOUND_TYPE.IT;

            CoreSystem.Native.createStream(dataPath + "m00.it", FMOD.MODE.LOOP_NORMAL, ref info, out FMOD.Sound sound);

            FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
            FMOD.Channel channel;
            CoreSystem.Native.playSound(sound, channelGroup, false, out channel);
            
            Marshal.FreeHGlobal(pathInt);
        }
    }
}