using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioListener : Component
    {
        public float volume;

        AL al;
        float[] orientation;
        public AudioListener() { 
            al = Audio.GetAL();
            volume = 1.0f;
            orientation = [0.0f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f];
        }

        public override void Start()
        {
            al.SetListenerProperty(ListenerFloat.Gain, volume);
        }

        public override void Update(float deltaTime)
        {
            //Todo hacer que pille la orientacion el metodo
            UpdateOrientation();
            al.SetListenerProperty(ListenerVector3.Position,gameObject.transform.position);
        }

        unsafe void UpdateOrientation()
        {
            Vector3 direction = gameObject.transform.TransformDirection(Vector3.UnitZ);
            orientation[0] = direction.X;
            orientation[1] = direction.Y;
            orientation[2] = direction.Z;
            fixed (float* p = orientation) {
                al.SetListenerProperty(ListenerFloatArray.Orientation,p);
            }
        }


    }
}
