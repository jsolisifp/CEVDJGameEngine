using Silk.NET.OpenGL;
using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioClipLoader : AssetLoader
    {
        public AudioClipLoader()
        {

        }

        public override object LoadAsset(string path)
        {
            return new AudioClip(path);
        }

        public override void UnloadAsset(object audioClip)
        {
            AudioClip ac = (AudioClip)audioClip;
            Audio.al.DeleteSource(ac.GetSource());
            Audio.al.DeleteBuffer(ac.GetBuffer());
        }
    }
}
