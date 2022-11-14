using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using MeltySynth;

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

        public MidiPlayer(string soundFontPath)
        {
            synthesizer = new Synthesizer(soundFontPath, sampleRate);
            sequencer = new MidiFileSequencer(synthesizer);

            dynamicSound = new DynamicSoundEffectInstance(sampleRate, AudioChannels.Stereo);
            buffer = new byte[4 * bufferLength];

            dynamicSound.BufferNeeded += (s, e) => SubmitBuffer();
        }

        public void Play(MidiFile midiFile, bool loop)
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

        public SoundState State => dynamicSound.State;
    }
}
