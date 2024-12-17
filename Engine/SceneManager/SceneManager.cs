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

        static GameObject CreateGameObject(string name, bool @static = false, Vector3 position = new Vector3(), Vector3 rotation = new Vector3())
        {
            GameObject g = new GameObject();

            g.name = name;
            g.@static = @static;
            g.AddComponent(new Transform());
            g.transform.position = position;
            g.transform.rotation = rotation;
            g.transform.scale = Vector3.One;

            return g;
        }

        static GameObject AddRenderer(GameObject g, string model = "UnitBox.obj", string shader = "Default.shader", string texture = "Texture1.png")
        {
            Renderer c = new Renderer();
            c.modelId = model;
            c.shaderId = shader;
            c.textureId = texture;
            g.AddComponent(c);
            return g;
        }

        static GameObject AddRigidbody(GameObject g, bool isKinematic = false, float mass = 1.0f, Vector3 speed = new Vector3(), Vector3 angularSpeed = new Vector3())
        {
            Rigidbody c = new Rigidbody();
            c.mass = mass;
            c.isKinematic = isKinematic;
            c.speed = speed;
            c.angularSpeed = angularSpeed;
            g.AddComponent(c);
            return g;
        }

        static GameObject AddTrigger(GameObject g)
        {
            Trigger c = new Trigger();
            g.AddComponent(c);
            return g;
        }

        static GameObject AddBoxCollider(GameObject g, Vector3 size)
        {
            BoxCollider c = new BoxCollider();
            c.size = size;
            g.AddComponent(c);
            return g;
        }

        static GameObject AddSphereCollider(GameObject g, float radius)
        {
            SphereCollider c = new SphereCollider();
            c.radius = radius;
            g.AddComponent(c);
            return g;
        }

        static GameObject AddCamera(GameObject g, float fov = 30, Vector3 backgroundColor = new Vector3())
        {
            Camera c = new Camera();
            c.fov = fov;
            c.backgroundColor = backgroundColor;
            g.AddComponent(c);
            return g;
        }

        static GameObject AddDirectionalLight(GameObject g, float intensity = 1.0f)
        {
            DirectionalLight c = new DirectionalLight();
            c.color = Vector3.One;
            c.intensity = intensity;
            g.AddComponent(c);
            return g;
        }

        static Scene CreateDefaultScene()
        {
            Scene scene = new Scene();

            scene.name = "New scene";

            GameObject go;
            Component c;
            Renderer rendererC;
            Camera cameraC;
            FollowCamera followC;
            DirectionalLight directionalLightC;
            GameObject sirMartinO;
            GameObject lizzardO;
            BoxCollider boxC;
            SphereCollider sphereC;
            Rigidbody rigidC;
            Trigger triggerC;

            go = CreateGameObject("DirectionalLight", false, Vector3.Zero, new Vector3(45, 0, 0));
            AddDirectionalLight(go);
            scene.AddGameObject(go);

            go = CreateGameObject("MainCamera", false, new Vector3(0, 0.5f, 1.5f), new Vector3(-10, 0, 0));           
            AddCamera(go);
            scene.AddGameObject(go);


            go = CreateGameObject("BowlingCentralLane");
            AddRenderer(go, "BowlingCentralLane.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingCover", true);
            AddRenderer(go, "BowlingCover.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingGround", true);
            AddRenderer(go, "BowlingGround.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingLaneLeft", true);
            AddRenderer(go, "BowlingLaneLeft.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingLaneRight", true);
            AddRenderer(go, "BowlingLaneRight.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingMarksArrows", true);
            AddRenderer(go, "BowlingMarksArrows.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingMarksDots", true);
            AddRenderer(go, "BowlingMarksDots.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingPlacedPins", true);
            AddRenderer(go, "BowlingPlacedPins.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("BowlingPlatform, true");
            AddRenderer(go, "BowlingPlatform.obj");
            scene.AddGameObject(go);

            go = CreateGameObject("Floor", true, new Vector3(0, -0.1f, 0));
            AddBoxCollider(go, new Vector3(100, 0.2f, 100));
            AddRigidbody(go);
            scene.AddGameObject(go);

            go = CreateGameObject("Ball", false, new Vector3(0, 0.5f, 0));
            AddRenderer(go, "BowlingBall.obj");
            AddSphereCollider(go, 0.12f);
            AddRigidbody(go, false);
            scene.AddGameObject(go);

            go = CreateGameObject("HandRight", false, new Vector3(0, 0.260f, 0));
            AddRenderer(go, "HandRight.obj");
            AddSphereCollider(go, 0.12f);
            AddTrigger(go);
            go.AddComponent(new Hand());
            scene.AddGameObject(go);
            
            return scene;

        }

    }
}
