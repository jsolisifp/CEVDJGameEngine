using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioSource : Component
    {

        public Vector3 offsetPosition;

        public string clipId;
        public float volume;
        public float pitch;
        public bool loop;
        public float maxDistance;
        public float rolloffFactor;
        public bool autoPlay;

        uint source;
        AL al;
        bool sourceStarted;
        ClipState clipState;
        public AudioSource()
        {
            al = Audio.GetAL();
            clipId = "";
            volume = 0.25f;
            pitch = 1.0f;
            loop = false;
            maxDistance = float.PositiveInfinity;
            rolloffFactor = 1.0f;
            clipState = ClipState.stoped;
            autoPlay = false;

            sourceStarted = false;
        }

        public override void Update(float deltaTime)
        {
            if (sourceStarted)
            {
                AudioClip audioClip = Assets.GetLoadedAsset<AudioClip>(clipId);
                if (audioClip != null) al.SetSourceProperty(source, SourceInteger.Buffer, audioClip.GetBuffer());
                al.SetSourceProperty(source, SourceVector3.Position, gameObject.transform.position);
                al.SetSourceProperty(source, SourceFloat.MaxGain, volume);
                al.SetSourceProperty(source, SourceFloat.Pitch, pitch);
                al.SetSourceProperty(source, SourceBoolean.Looping, loop);
                al.SetSourceProperty(source, SourceFloat.MaxDistance, maxDistance);
                al.SetSourceProperty(source, SourceFloat.RolloffFactor, rolloffFactor);

                if (autoPlay && Engine.GetState() == Engine.State.playing) { PlayAudio(); autoPlay = false; }
            }
        }


        public override void Start()
        {
            
            Console.WriteLine("Cargado");


            sourceStarted = true;
            source = Audio.GetAL().GenSource();

            

        }
        public override void Stop()
        {
            if (!sourceStarted) { return; }
            sourceStarted = false;
            al.SourceStop(source);
            al.DeleteSource(source);
        }

        public void PlayAudio()
        {
            if(!sourceStarted) Start();
            if(!sourceStarted || clipState == ClipState.playing) return;
            clipState = ClipState.playing;
            al.SourcePlay(source);
        }
        public void StopAudio()
        {
            clipState = ClipState.stoped;
            al.SourceStop(source);
        }
        public void PauseAudio()
        {
            clipState = ClipState.paused;
            al.SourcePause(source);
        }
        public void ResumeAudio()
        {
            clipState = ClipState.playing;
            al.SourcePlay(source);
        }

        public bool GetSourceStarted() { return sourceStarted; }
        public ClipState GetClipState() { return clipState; }
    }
}
