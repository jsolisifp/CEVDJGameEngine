using Silk.NET.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{

    public enum ClipState
    {
        playing,
        paused,
        stoped
    }
    internal class Audio
    {
        static ALContext alc;
        static AL al;
        static unsafe Device* device;
        static unsafe Context* context;
        static DistanceModel atenuation;

        private static bool renderAudioEnabled;

        public static unsafe void Init()
        {
            alc = ALContext.GetApi();
            al = AL.GetApi();

            Assets.RegisterAssetLoader("wav", new AudioClipLoader());

            device = alc.OpenDevice("");
            if (device == null)
            {
                Console.WriteLine("Could not create device");
                return;
            }

            context = alc.CreateContext(device, null);
            alc.MakeContextCurrent(context);
            atenuation = DistanceModel.LinearDistance;
            al.DistanceModel(atenuation);

            Render.onRenderOverlay += RenderAudioComponents;

            renderAudioEnabled = true;
            al.GetError();
        }

        public static unsafe void Finish()
        {
            alc.DestroyContext(context);
            alc.CloseDevice(device);
            al.Dispose();
            alc.Dispose();
        }

        public static ALContext GetALContext()
        {
            return alc;
        }

        public static AL GetAL()
        {
            return al;
        }

        public static void SetAtenuation(DistanceModel distanceModel)
        {
            atenuation = distanceModel;
            al.DistanceModel(distanceModel);
        }

        public static DistanceModel GetAtenuation()
        {
            return atenuation;
        }

        
        public static void RenderAudioComponents(float deltaTime)
        {
            /*
            Model m = Assets.GetLoadedAsset<Model>("UnitSphere.obj");
            Texture t = Assets.GetLoadedAsset<Texture>("Green.png");
            Texture t1 = Assets.GetLoadedAsset<Texture>("Red.png");
            Texture t2 = Assets.GetLoadedAsset<Texture>("Gray.png");
            Shader s = Assets.GetLoadedAsset<Shader>("DefaultTransparent.shader");

            if (!renderAudioEnabled) return;

            Render.ClearDepth();
            Render.SetOpacity(0.5f);
            
            List<Component> listeners = Editor.GetComponentsOfType<AudioListener>();
            if (listeners != null && m != null && t != null && s != null)
            {
                AudioListener audioListener;
                for (int i = 0; i < listeners.Count; i++)
                {
                    audioListener = (AudioListener) listeners[i];

                    Transform tf = audioListener.GetGameObject().transform;
                    Render.DrawModel(tf.position, tf.rotation, tf.scale, m, s, t);
                }
            }

            List<Component> sources = Editor.GetComponentsOfType<AudioSource>();
            if (sources != null && m != null && t1 != null && s != null)
            {
                AudioSource source;
                for (int i = 0; i < sources.Count; i++)
                {
                    source = (AudioSource)sources[i];

                    Transform tf = source.GetGameObject().transform;
                    Render.DrawModel(tf.position, tf.rotation, tf.scale, m, s, t1);

                    if (source.maxDistance != float.PositiveInfinity && t2 != null)
                    {
                        Render.DrawModel(tf.position, tf.rotation, new System.Numerics.Vector3(source.maxDistance * 2), m, s, t2);
                    }

                }
            }*/
        }

        public static bool GetRenderAudioEnabled()
        {
            return renderAudioEnabled;
        }

        public static void SetRenderAudioEnabled(bool value)
        {
            renderAudioEnabled = value;
        }
    }
}
