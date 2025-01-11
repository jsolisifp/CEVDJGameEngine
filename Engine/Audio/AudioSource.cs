using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameEngine
{
    internal class AudioSource : Component
    {
        public AudioListener listener;

        public string clipId;
        uint source;

        public bool loop = false;
        public float volume = 0.5f;
        public float pitch = 1;

        public float maxDistance = 100;
        public float referenceDistance = 1;
        public float rolloffFactor = 1.1f;

        float[] orientation = { 0, 0, -1f, 0, 1f, 0f };

        public unsafe override void Start()
        {
            AudioClip clip = Assets.GetLoadedAsset<AudioClip>(clipId);
            source = clip.GetSource();

            Audio.al.DistanceModel(DistanceModel.LinearDistanceClamped);
           // Audio.al.SetListenerProperty(ListenerFloat.Gain, listener.volume);

            Audio.al.SetSourceProperty(source, SourceFloat.Gain, volume);
            Audio.al.SetSourceProperty(source, SourceBoolean.Looping, loop);
            Audio.al.SetSourceProperty(source, SourceFloat.Pitch, pitch);

            Audio.al.SetSourceProperty(source, SourceFloat.MaxDistance, maxDistance);
            Audio.al.SetSourceProperty(source, SourceFloat.ReferenceDistance, referenceDistance);
            Audio.al.SetSourceProperty(source, SourceFloat.RolloffFactor, rolloffFactor);
        }
        public unsafe override void Update(float deltatime)
        {
            //Audio.al.SetListenerProperty(ListenerVector3.Position, listener.transform.position);
            //fixed (float* o = orientation)
            //{
            //    Audio.al.SetListenerProperty(ListenerFloatArray.Orientation, o);
            //}

            Audio.al.SetSourceProperty(source, SourceVector3.Position, this.gameObject.transform.position);
            Audio.al.SetSourceProperty(source, SourceVector3.Direction, this.gameObject.transform.rotation);
        }

        public void Play()
        {
            if (!Audio.audioEnabled) return;
            Audio.al.SourcePlay(source);
        }
        public void Pause()
        {
            if (!Audio.audioEnabled) return;
            Audio.al.SourcePause(source);
        }
        public void Resume()
        {
            if (!Audio.audioEnabled) return;
            Audio.al.SourcePlay(source);
        }
        public void Stop()
        {
            if (!Audio.audioEnabled) return;
            Audio.al.SourceStop(source);
        }
    }
}
