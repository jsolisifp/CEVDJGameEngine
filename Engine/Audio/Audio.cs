using Silk.NET.Assimp;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal unsafe class Audio
    {
        public static AL al;
        public static ALContext alc;
        public static Device* device;
        public static Context* alContext;

        public static bool audioEnabled;

        public static void Init()
        {
            audioEnabled = false;

            Assets.RegisterAssetLoader("wav", new AudioClipLoader());

            al = AL.GetApi();
            alc = ALContext.GetApi();

            device = alc.OpenDevice("");
            if (device == null)
            {
                Console.WriteLine("Could not create device");
                return;
            }

            alContext = alc.CreateContext(device, null);
            alc.MakeContextCurrent(alContext);

            audioEnabled = true;
        }
        public static void Finish()
        {
            alc.DestroyContext(alContext);
            alc.CloseDevice(device);
            al.Dispose();
            alc.Dispose();
        }

        public static void SetDistanceModel(int model)
        {
            switch (model)
            {
                case 0:
                    al.DistanceModel(DistanceModel.None);
                    break;
                case 1:
                    al.DistanceModel(DistanceModel.InverseDistance);
                    break;
                case 2:
                    al.DistanceModel(DistanceModel.InverseDistanceClamped);
                    break;
                case 3:
                    al.DistanceModel(DistanceModel.LinearDistance);
                    break;
                case 4:
                    al.DistanceModel(DistanceModel.LinearDistanceClamped);
                    break;
                case 5:
                    al.DistanceModel(DistanceModel.ExponentDistance);
                    break;
                case 6:
                    al.DistanceModel(DistanceModel.ExponentDistanceClamped);
                    break;
            }
        }
    }
}
