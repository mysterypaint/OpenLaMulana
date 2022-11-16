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

        public MidiPlayer(string soundFontPath, SynthesizerSettings settings)
        {
            synthesizer = new Synthesizer(soundFontPath, settings);

            sequencer = new MidiFileSequencer(synthesizer);

            dynamicSound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);
            buffer = new byte[4 * bufferLength];

            dynamicSound.BufferNeeded += (s, e) => SubmitBuffer();
        }

        public void Play(MidiFile midiFile, bool loop = false)
        {
            sequencer.Play(midiFile, loop);

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

        public SoundState State => dynamicSound.State;
    }
}
