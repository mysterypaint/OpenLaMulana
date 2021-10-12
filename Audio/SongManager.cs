using SharpMik;
using SharpMik.Player;
using SharpMik.Drivers;
using System;
using System.Diagnostics;

namespace OpenLaMulana
{
    public class SongManager
    {
        Module m_Mod = null;
        MikMod m_Player;
        bool m_Playing = false;
        bool m_WasPlaying = false;

        public SongManager()
        {
            m_Player = new MikMod();
        }

        public void InitPlayer()
        {
            ModDriver.Mode = (ushort)(ModDriver.Mode | SharpMikCommon.DMODE_NOISEREDUCTION);
            
            m_Player.Init<SharpMik.Drivers.NaudioDriver>("");

            DateTime start = DateTime.Now;

        }

        public void LoadSong(string fileName)
        {
            m_Mod = m_Player.LoadModule(fileName);
            m_Playing = false;
            m_WasPlaying = false;
        }

        public void TogglePause()
        {
            if (m_Playing)
            {
                m_Playing = false;
                m_WasPlaying = true;
                ModPlayer.Player_TogglePause();
            }
            else
            {
                m_Playing = true;

                if (m_WasPlaying)
                {
                    ModPlayer.Player_TogglePause();
                }
                else
                {
                    m_Player.Play(m_Mod);
                }
            }
        }

        public void PlaySong(string fileName)
        {
            StopSong();
            LoadSong(fileName);
            TogglePause();
        }

        public void StopSong()
        {
            m_Playing = false;
            ModPlayer.Player_Stop();
        }

        public String PrintSongName()
        {
            if (m_Mod != null)
                return m_Mod.songname;
            else
                return "Song is undefined.";
        }

        public void Cleanup()
        {
            ModPlayer.Player_Stop();

            ModDriver.MikMod_Exit();
        }
    }
}