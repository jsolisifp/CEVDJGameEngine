using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace GameEngine
{
    internal class Engine
    {
        static IWindow window;

        public enum State
        {
            stopped,
            playing,
            paused
        };

        static State state;

        public static void Run()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "My Game Engine";
            window = Window.Create(options);

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Closing += OnClose;

            state = State.stopped;

            window.Run();

            window.Dispose();
        }

        public static State GetState()
        {
            return state;
        }

        public static void Pause()
        {
            #if DEBUG
            Debug.Assert(state == State.playing, "Engine only can pause state from playing state");
            #endif


            state = State.paused;
        }

        public static void Resume()
        {
            #if DEBUG
            Debug.Assert(state == State.paused, "Engine only can resume from paused state");
            #endif

            state = State.playing;
        }

        public static void Play()
        {
            #if DEBUG
            Debug.Assert(state == State.stopped, "Engine only can enter play state from stop");
            #endif

            SceneManager.Stop();
            Physics.Stop();

            // This stop and start is needed
            // because static, isKinematic
            // and other properties that the object captures
            // on start may have changed.


            Physics.Start();
            SceneManager.Start();
            state = State.playing;
        }

        public static void Stop()
        {
            #if DEBUG
            Debug.Assert(state == State.playing || state == State.paused, "Engine only can stop from playing or paused state");
            #endif

            SceneManager.Stop();
            Physics.Stop();

            SceneManager.Reload();

            Physics.Start();
            SceneManager.Start();
            state = State.stopped;
        }

        private static void OnRender(double deltaTime)
        {
            Render.OnRender((float)deltaTime);
        }

        static void OnLoad()
        {
            window.SetDefaultIcon();

            Assets.Init();
            Input.Init(window);
            Input.onKeyDown += OnKeyDown;
            SceneManager.Init(window);
            Render.Init(window);
            Physics.Init(window);

            Assets.LoadAssets();
            SceneManager.Start();
            Editor.Init(window, Input.GetContext(), Render.GetContext());

        }


        private static void OnUpdate(double deltaTime)
        {
            Assets.Update((float)deltaTime);
            Input.Update((float)deltaTime);

            if(state == State.playing)
            {
                SceneManager.Update((float)deltaTime);
                Physics.Update((float)deltaTime);
            }
            else
            {
                SceneManager.Update(0);
                Physics.Update(0, true);
            }

            Editor.Update((float)deltaTime);

        }

        private static void OnClose()
        {
            Editor.Finish();
            SceneManager.Stop();
            Assets.UnloadAssets();
            Physics.Finish();
            Render.Finish();
            SceneManager.Finish();
            Input.onKeyDown -= OnKeyDown;
            Input.Finish();
            Assets.Finish();

            window.Closing -= OnClose;
            window.Render -= OnRender;
            window.Update -= OnUpdate;
            window.Load -= OnLoad;
        }

        static void OnKeyDown(Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                window.Close();
            }
        }
    }
}
