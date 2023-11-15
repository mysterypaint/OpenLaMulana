using MeltySynth;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Runtime.InteropServices;

namespace OpenLaMulana.Audio
{
    public class MidiPlayer : IDisposable
    {
        private static readonly int sampleRate = 44100;
        private static readonly int bufferLength = sampleRate / 10;

        private Synthesizer synthesizer;
        private MidiFileSequencer sequencer;

        private DynamicSoundEffectInstance dynamicSound;
        private byte[] buffer;
        private bool _noMusic = false;

        public MidiPlayer(string soundFontPath, SynthesizerSettings settings)
        {
            synthesizer = new Synthesizer(soundFontPath, settings);

            sequencer = new MidiFileSequencer(synthesizer);

            try
            {
                dynamicSound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);
                buffer = new byte[4 * bufferLength];

                dynamicSound.BufferNeeded += (s, e) => SubmitBuffer();
            }
            catch (NoAudioHardwareException)
            {
                _noMusic = true;
            }
        }

        public void Play(MidiFile midiFile, int songID, bool loop = false)
        {
            if (_noMusic)
                return;
            sequencer.Play(midiFile, songID, loop);

            if (dynamicSound.State != SoundState.Playing)
            {
                SubmitBuffer();
                dynamicSound.Play();
            }
        }

        public void Stop()
        {
            sequencer.Stop();
        }

        private void SubmitBuffer()
        {
            if (_noMusic)
                return;
            sequencer.RenderInterleavedInt16(MemoryMarshal.Cast<byte, short>(buffer));
            dynamicSound.SubmitBuffer(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            if (dynamicSound != null)
            {
                dynamicSound.Dispose();
                dynamicSound = null;
            }
        }

        internal void Pause()
        {
            sequencer.Speed = 0;
            sequencer.NoteOffAll(true);
        }

        internal void Resume()
        {
            sequencer.Speed = 1;
            sequencer.NoteOffAll(true);
        }

        internal void SetMasterVolume(float newVal)
        {
            synthesizer.MasterVolume = newVal;
        }
        internal float GetMasterVolume()
        {
            return synthesizer.MasterVolume;
        }

        internal void ToggleChannel(int channelID)
        {
            synthesizer.ToggleChannel(channelID);
        }

        internal float GetChannelVolume(int channelID)
        {
            return synthesizer.GetChannelVolume(channelID);
        }

        internal UInt32 GetEnabledChannels()
        {
            return synthesizer.GetEnabledChannels();
        }

        internal void SetEnabledChannels(UInt32 value)
        {
            synthesizer.SetEnabledChannels(value);
        }

        public SoundState State => dynamicSound.State;
    }
}
