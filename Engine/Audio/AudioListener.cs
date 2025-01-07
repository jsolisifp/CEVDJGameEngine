using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GameEngine;
using System.Diagnostics;

namespace GameEngine
{
    //Es como el micro de la cámara... 
    internal class AudioListener : Component
    {
        public float volume;
        public AL al;

        public AudioListener()
        {
            al = Audio.GetAL();
        }
        public override void Update(float dt)
        {
            al.SetListenerProperty(Silk.NET.OpenAL.ListenerVector3.Position, gameObject.transform.position);
            UpdateDirection();
        }

        unsafe void UpdateDirection()
        {
            Vector3 direction = gameObject.transform.TransformDirection(Vector3.UnitZ * -1);
            float[] direction2 = { direction.X, direction.Y, direction.Z, 0, 1, 0 };

            fixed (float * q = direction2)
            {
                al.SetListenerProperty(ListenerFloatArray.Orientation, q);
            }
        }
    }


}

