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
            GameObject sirMartinO;
            GameObject lizzardO;
            BoxCollider boxC;
            SphereCollider sphereC;
            Rigidbody rigidC;
            Trigger triggerC;

            go = new GameObject();
            go.name = "DirectionalLight";
            go.AddComponent(new Transform());

            directionalLightC = new DirectionalLight();
            directionalLightC.color = new Vector3(1, 0.961f, 0.753f);
            directionalLightC.intensity = 1.0f;
            go.transform.rotation = new Vector3(45, 0, 0);

            go.AddComponent(directionalLightC);

            scene.AddGameObject(go);

            string[] props = {"", };

            string[] propsNames = {"CoverLane", };


            go = new GameObject();
            go.name = propsNames[i];
            go.AddComponent(new Transform());

            rendererC = new Renderer();
            rendererC.modelId = props[i];
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Texture1.png";

            go.AddComponent(rendererC);
            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "GroundCollider";
            go.AddComponent(new Transform());
            go.@static = true;
            boxC = new BoxCollider();
            boxC.size = new Vector3(100,0.2f,100);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;
            rigidC.mass = 1;
            go.AddComponent(rigidC);

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "HandRight";
            go.AddComponent(new Transform());

            rendererC = new Renderer();
            rendererC.modelId = "HandleRight.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Texture1.png";
            go.AddComponent(rendererC);

            boxC = new BoxCollider();
            boxC.size = new Vector3(0.5f, 0.01f, 0.12f);
            go.AddComponent(boxC);

            go.AddComponent(new Trigger());







            go = new GameObject();
            go.name = "MainCamera";
            go.AddComponent(new Transform());

            go.transform.position = new Vector3(0, 0.6f, 2);
            go.transform.rotation = new Vector3(-20, 0, 0);
            cameraC = new Camera();
            go.AddComponent(cameraC);
            scene.AddGameObject(go);
            
            return scene;

        }

    }
}
