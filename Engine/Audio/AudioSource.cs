using Silk.NET.Assimp;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace GameEngine
{
    unsafe class AudioSource : Component
    {        
        public string clipId;
        public float volume;
        public float pitch;
        public bool loop;
        public float maxDistance;
        public float rolloffFactor;
        AL openAl;

        public AudioSource()
        {
            openAl = Audio.GetAL();
            clipId = "";
            volume = 1;
            pitch = 1;
            loop = true;
            maxDistance = 10;
            rolloffFactor = 1.0f;
            state = 0;
        }

        
        uint source;
        bool isInitilized;
        int state; //0 stopped, 1 play, 2 pause
                
        unsafe void Initialize()
        {
            AudioClip audioClip = Assets.GetLoadedAsset<AudioClip>(clipId);
            if (audioClip == null || openAl == null) { return; }
          
            source = openAl.GenSource();
            openAl.SetSourceProperty(source, SourceBoolean.Looping, loop);
            openAl.SetSourceProperty(source, SourceFloat.MaxGain, volume);
            openAl.SetSourceProperty(source, SourceFloat.Pitch, pitch);
            openAl.SetSourceProperty(source, SourceFloat.MaxDistance, maxDistance);
            openAl.SetSourceProperty(source, SourceFloat.RolloffFactor, rolloffFactor);
            openAl.SetSourceProperty(source, SourceInteger.Buffer, audioClip.buffer);

            isInitilized = true;
        }

        public override void Update(float deltaTime)
        {
            if (isInitilized)
            {
                openAl.SetSourceProperty(source, SourceVector3.Position, gameObject.transform.position);
            }
        }

        public int GetState()
        {
            return state;
        }

        public override void Start()
        {
            Initialize();
        }

        public void Play()
        {
            if (!isInitilized)
            {
                Start();
            }
            if (!isInitilized)
            {
                return;
            }

            openAl.SourcePlay(source);

            state = 1;
        }

        public override void Stop()
        {
            if (isInitilized)
            {
                openAl.SourceStop(source);
                openAl.DeleteSource(source);
                isInitilized = false;

                state = 0;
            }

        }

        public void Pause()
        {
            openAl.SourcePause(source);

            state = 2;
        }

        public void Resume()
        {
            Play();
        }

        public void Dispose()
        {

        }
    }
}
