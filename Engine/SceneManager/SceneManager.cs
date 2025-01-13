using GameEngine.Game;
using Silk.NET.Assimp;
using Silk.NET.Windowing;
using System.Numerics;

namespace GameEngine
{
    internal class SceneManager
    {
        static string activeSceneId;

        static Scene ActiveScene
        { 
            get { return activeSceneId == null ? defaultScene : Assets.GetLoadedAsset<Scene>(activeSceneId); }
        }

        static IWindow window;

        static bool isPaused;

        static Scene defaultScene;

        public static void Init(IWindow _window)
        {
            SceneLoader loader = new SceneLoader();
            Assets.RegisterAssetLoader("scene", loader);

            window = _window;

            defaultScene = CreateDefaultScene();

            activeSceneId = null;

            isPaused = false;
        }

        public static void Finish()
        {
        }

        public static void Start()
        {
            ActiveScene.Start();

        }

        public static void Stop()
        {
            ActiveScene.Stop();
        }

        public static void Reload()
        {
            if(activeSceneId != null) { Assets.ReloadAsset(activeSceneId); }
            else { defaultScene = CreateDefaultScene(); }
        }

        public static void New()
        {
            ActiveScene.Stop();

            defaultScene = CreateDefaultScene();
            activeSceneId = null;
            
            ActiveScene.Start();
        }

        public static void Update(float deltaTime)
        {
            ActiveScene.Update(deltaTime);
        }

        public static void FixedUpdate(float deltaTime)
        {
            ActiveScene.FixedUpdate(deltaTime);
        }

        public static void Render(float deltaTime)
        {
            ActiveScene.Render(deltaTime);
        }

        public static void SetActiveSceneAssetId(string assetId)
        {
            ActiveScene.Stop();

            activeSceneId = assetId;

            ActiveScene.Start();
        }

        public static Scene GetActiveScene()
        {
            return ActiveScene;
        }

        public static string GetActiveSceneAssetId()
        {
            return activeSceneId;
        }

        public static Scene CreateDefaultScene()
        {
            Scene scene = new Scene();

            scene.name = "New scene";

            GameObject go;
            Component c;
            Renderer rendererC;
            Camera cameraC;
            FollowCamera followC;
            DirectionalLight directionalLightC;
            BoxCollider boxC;
            SphereCollider sphereC;
            Rigidbody rigidC;
            Trigger triggerC;
            CameraMainScript cameraMSC;
            GameManager gameManagerC;
            AudioSource audioSourceC;

            // Game Manager

            go = new GameObject();
            go.name = "GameManager";
            go.AddComponent(new Transform());

            AudioListener audioListener = new AudioListener();
            go.AddComponent(audioListener);

            audioSourceC = new AudioSource();
            audioSourceC.clipId = "BowlingWiiMusic.wav";
            audioSourceC.listener = audioListener;
            audioSourceC.loop = true;
            go.AddComponent(audioSourceC);

            gameManagerC = new GameManager();
            gameManagerC.audioSource = audioSourceC;
            go.AddComponent(gameManagerC);

            scene.AddGameObject(go);

            // Directional Light

            go = new GameObject();
            go.name = "DirectionalLight";
            go.AddComponent(new Transform());

            directionalLightC = new DirectionalLight();
            directionalLightC.color = new Vector3(1, 0.961f, 0.753f);
            directionalLightC.intensity = 1.0f;
            go.transform.rotation = new Vector3(45, 0, 0);
            go.AddComponent(directionalLightC);

            scene.AddGameObject(go);

            // Main Camera

            go = new GameObject();
            go.name = "MainCamera";
            go.AddComponent(new Transform());
            go.transform.position = new Vector3(0, 1, 2);
            go.transform.rotation = new Vector3(-20, 0, 0);

            cameraC = new Camera();
            go.AddComponent(cameraC);

            cameraMSC = new CameraMainScript();
            go.AddComponent(cameraMSC);

            scene.AddGameObject(go);

            // Props

            string[] props =
            {
                "BowlingCentralLane.obj",
                //"BowlingCover.obj",
                "BowlingGround.obj",
                "BowlingLaneLeft.obj",
                "BowlingLaneRight.obj",
                "BowlingMarksArrows.obj",
                "BowlingMarksDots.obj",
                "BowlingPlacedPins.obj",
                "BowlingPlatform.obj"
            };

            string[] propsNames =
            {
                "BowlingCentralLane",
                //"BowlingCover",
                "BowlingGround",
                "BowlingLaneLeft",
                "BowlingLaneRight",
                "BowlingMarksArrows",
                "BowlingMarksDots",
                "BowlingPlacedPins",
                "BowlingPlatform"
            };

            for (int i = 0; i < props.Count(); i++)
            {
                go = new GameObject();
                go.name = propsNames[i];
                go.AddComponent(new Transform());

                rendererC = new Renderer();
                rendererC.modelId = props[i];
                rendererC.shaderId = "Default.shader";
                if (propsNames[i] == "BowlingCentralLane" || propsNames[i] == "BowlingCover" || propsNames[i] == "BowlingPlatform")
                {
                    rendererC.textureId = "MarronClaro.png";
                }
                else if (propsNames[i] == "BowlingMarksArrows" || propsNames[i] == "BowlingMarksDots" || propsNames[i] == "BowlingLaneLeft" || propsNames[i] == "BowlingLaneRight")
                {
                    rendererC.textureId = "Marron.png";
                }
                else
                {
                    rendererC.textureId = "Texture1.png";
                }
                
                go.AddComponent(rendererC);

                scene.AddGameObject(go);
            }

            // Pins

            for (int i = 1; i <= 10; i++)
            {
                go = new GameObject();
                go.name = "BowlingPin" + i;
                go.AddComponent(new Transform());

                switch (i)
                {
                    case 1:
                        go.transform.position = new Vector3(0, 0.19f, -22.86f);
                        break;
                    case 2:
                        go.transform.position = new Vector3(-0.153f, 0.19f, -23.124f);
                        break;
                    case 3:
                        go.transform.position = new Vector3(0.153f, 0.19f, -23.124f);
                        break;
                    case 4:
                        go.transform.position = new Vector3(-0.305f, 0.19f, -23.387f);
                        break;
                    case 5:
                        go.transform.position = new Vector3(0, 0.19f, -23.387f);
                        break;
                    case 6:
                        go.transform.position = new Vector3(0.305f, 0.19f, -23.387f);
                        break;
                    case 7:
                        go.transform.position = new Vector3(-0.458f, 0.19f, -23.65f);
                        break;
                    case 8:
                        go.transform.position = new Vector3(-0.105f, 0.19f, -23.65f);
                        break;
                    case 9:
                        go.transform.position = new Vector3(0.153f, 0.19f, -23.65f);
                        break;
                    case 10:
                        go.transform.position = new Vector3(0.458f, 0.19f, -23.65f);
                        break;
                    default:
                        break;
                }

                rendererC = new Renderer();
                rendererC.modelId = "BowlingPin.obj";
                rendererC.shaderId = "Default.shader";
                rendererC.textureId = "Blanco.png";
                go.AddComponent(rendererC);

                rigidC = new Rigidbody();
                rigidC.isKinematic = false;
                rigidC.mass = 1.5f;
                go.AddComponent(rigidC);

                boxC = new BoxCollider();
                boxC.size = new Vector3(0.12f, 0.38f, 0.12f);
                go.AddComponent(boxC);

                scene.AddGameObject(go);
            }

            // Ball

            go = new GameObject();
            go.name = "Ball";
            go.AddComponent(new Transform());
            go.transform.position = new Vector3(0, 1, 0);

            rendererC = new Renderer();
            rendererC.modelId = "BowlingBall.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Negro.png";
            go.AddComponent(rendererC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = false;
            rigidC.mass = 7;
            go.AddComponent(rigidC);

            sphereC = new SphereCollider();
            sphereC.radius = 0.1f;
            go.AddComponent(sphereC);

            go.AddComponent(new Ball());

            cameraMSC.ball = go.transform;

            scene.AddGameObject(go);

            // Hand Right

            go = new GameObject();
            go.name = "HandRight";
            go.AddComponent(new Transform());
            go.transform.position = new Vector3(0, 0, 0);
            go.transform.scale = new Vector3(1.5f, 1.5f, 1.5f);

            rendererC = new Renderer();
            rendererC.modelId = "HandRight.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Texture1.png";
            go.AddComponent(rendererC);

            boxC = new BoxCollider();
            boxC.size = new Vector3(0.1f, 0.02f, 0.24f);
            go.AddComponent(boxC);

            go.AddComponent(new Trigger());

            go.AddComponent(new Hand());

            scene.AddGameObject(go);

            // Ground Collider

            go = new GameObject();
            go.name = "GroundCollider";
            go.AddComponent(new Transform());

            go.@static = true;

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;
            rigidC.mass = 1;
            rigidC.friction = 1.2f;
            go.AddComponent(rigidC);

            boxC = new BoxCollider();
            boxC.size = new Vector3(100,0.0f,100);
            go.AddComponent(boxC);

            scene.AddGameObject(go);

            // Pins 1-10

            return scene;

        }

    }
}
