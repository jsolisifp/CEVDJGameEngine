using Silk.NET.Core.Contexts;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Audio
    {
        static ALContext alC;
        static AL al;

        static unsafe Context* context;
        static unsafe Device* device;

        private static bool isInitialized = false;

        public static unsafe void Init()
        {
            if (isInitialized)
            {
                return;
            }

            alC = ALContext.GetApi();
            al = AL.GetApi();
            Assets.RegisterAssetLoader("wav", new AudioClipLoader(al));

            device = alC.OpenDevice("");
            context = alC.CreateContext(device, null);
            alC.MakeContextCurrent(context);
            al.DistanceModel(DistanceModel.ExponentDistance);
            al.GetError();

            isInitialized = true;
            Console.WriteLine("Audio system initialized.");
        }

        public static AL GetAL()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Audio system not initialized. Please call Audio.Init() before using.");
            }

            return al;
        }

        public static ALContext GetALContext()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Audio system not initialized. Please call Audio.Init() before using.");
            }

            return alC;
        }

        public static unsafe void Finish()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Audio system not initialized.");
            }

            alC.DestroyContext(context);
            alC.CloseDevice(device);
            al.Dispose();
            alC.Dispose();

            isInitialized = false;
            Console.WriteLine("Audio system cleaned up.");
        }
    }
}
