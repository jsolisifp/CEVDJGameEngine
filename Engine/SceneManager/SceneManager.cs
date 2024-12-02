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
            Lizzard lizzardC;
            GameObject lizzardO;
            BoxCollider boxC;
            SphereCollider sphereC;
            Rigidbody rigidC;

            // Directional light

            go = new GameObject();
            go.name = "DirectionalLight";
            go.AddComponent(new Transform());

            directionalLightC = new DirectionalLight();
            directionalLightC.color = new Vector3(1, 0.961f, 0.753f);
            directionalLightC.intensity = 1.0f;
            go.transform.rotation = new Vector3(45, 0, 0);

            go.AddComponent(directionalLightC);

            scene.AddGameObject(go);

            // Terrain

            go = new GameObject();
            go.name = "Terrain";
            go.AddComponent(new Transform());

            rendererC = new Renderer();
            rendererC.modelId = "Terrain.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Grass.png";

            go.AddComponent(rendererC);

            scene.AddGameObject(go);

            // Floor


            go = new GameObject();
            go.name = "Floor";
            go.@static = true;
            go.AddComponent(new Transform());

            boxC = new BoxCollider();
            boxC.size = new Vector3(500, 1, 500);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();

            go.AddComponent(rigidC);

            go.transform.position = new Vector3(0, -0.5f, 0);
            go.transform.rotation = new Vector3(0, 0, 0);

            scene.AddGameObject(go);


            // Sir Martin

            go = new GameObject();
            go.name = "SirMartin";
            go.AddComponent(new Transform());

            rendererC = new Renderer();
            rendererC.modelId = "Model2.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Texture2.png";

            go.AddComponent(rendererC);

            boxC = new BoxCollider();
            boxC.size = new Vector3(2, 1, 2);

            go.AddComponent(boxC);

            scene.AddGameObject(go);

            // Lizzard

            go = new GameObject();
            go.name = "Lizzard";
            go.AddComponent(new Transform());

            go.transform.position = new Vector3(10, 0, 0);

            boxC = new BoxCollider();
            boxC.size = new Vector3(2, 1, 2);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;

            go.AddComponent(rigidC);

            lizzardC = new Lizzard();

            go.AddComponent(lizzardC);

            rendererC = new Renderer();
            rendererC.modelId = "Lizzard.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Blue.png";

            go.AddComponent(rendererC);

            lizzardO = go;

            scene.AddGameObject(go);

            // Main camera

            go = new GameObject();
            go.name = "MainCamera";
            go.AddComponent(new Transform());

            cameraC = new Camera();
            go.AddComponent(cameraC);

            followC = new FollowCamera();

            followC.target = lizzardO.transform;

            go.AddComponent(followC);

            scene.AddGameObject(go);

            // Dynamic box

            go = new GameObject();
            go.name = ("DynamicBox");
            go.AddComponent(new Transform());

            go.transform.position = new Vector3(-5, 1, 5);
            go.transform.rotation = new Vector3(0, 0, 0);
            go.transform.scale = new Vector3(1, 1, 1);

            boxC = new BoxCollider();
            boxC.size = new Vector3(1, 1, 1);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = false;

            rigidC.speed = new Vector3(0, 0, 0);
            rigidC.angularSpeed = new Vector3(0, 0, 0);

            rigidC.mass = 10;

            go.AddComponent(rigidC);

            rendererC = new Renderer();
            rendererC.modelId = "UnitBox.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Wood.png";

            go.AddComponent(rendererC);

            scene.AddGameObject(go);

            // Dynamic sphere

            go = new GameObject();
            go.name = "DynamicSphere";
            go.AddComponent(new Transform());

            go.transform.position = new Vector3(5, 1, 5);
            go.transform.rotation = new Vector3(0, 0, 0);
            go.transform.scale = new Vector3(1, 1, 1);

            sphereC = new SphereCollider();
            sphereC.radius = 0.5f;

            go.AddComponent(sphereC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = false;
            rigidC.mass = 4 / 3 * MathF.PI * MathF.Pow(sphereC.radius, 3) * 10;

            rigidC.speed = new Vector3(0, 0, 0);
            rigidC.angularSpeed = new Vector3(0, 0, 0);

            go.AddComponent(rigidC);

            rendererC = new Renderer();
            rendererC.modelId = "UnitSphere.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Wood.png";

            go.AddComponent(rendererC);

            scene.AddGameObject(go);


            return scene;

        }

    }
}
