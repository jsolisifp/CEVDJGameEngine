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
            BoxCollider boxC;
            SphereCollider sphereC;
            Rigidbody rigidC;
            Trigger triggerC;
            Meka mekaC;
            Weapon weaponC;
            MekaController mekaControllerC;

            go = new GameObject();
            go.name = "DirectionalLight";
            go.AddComponent(new Transform());

            directionalLightC = new DirectionalLight();
            directionalLightC.color = new Vector3(1, 0.961f, 0.753f);
            directionalLightC.intensity = 1.0f;
            go.transform.rotation = new Vector3(45, 0, 0);

            go.AddComponent(directionalLightC);
            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "MainCamera";
            go.AddComponent(new Transform());

            cameraC = new Camera();
            go.AddComponent(cameraC);

            scene.AddGameObject(go);

            Random r = new Random(0);

            go = new GameObject();
            go.name = "Floor";
            go.@static = true;
            go.AddComponent(new Transform());

            boxC = new BoxCollider();
            boxC.size = new Vector3(250, 1, 250);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();

            go.AddComponent(rigidC);

            go.transform.position = new Vector3(0, -0.5f, 0);
            go.transform.scale = new Vector3(250, 1, 250);

            rendererC = new Renderer();
            rendererC.modelId = "UnitBox.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Wood.png";

            go.AddComponent(rendererC);

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "Meka";
            go.AddComponent(new Transform());
            mekaC = new Meka();
            go.AddComponent(mekaC);
            weaponC = new Weapon();
            go.AddComponent(weaponC);
            mekaC.leftWeapon = weaponC;
            weaponC = new Weapon();
            go.AddComponent(weaponC);
            mekaC.rightWeapon = weaponC;

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "MekaController";
            go.AddComponent(new Transform());
            mekaControllerC = new MekaController();
            go.AddComponent(mekaControllerC);
            mekaControllerC.mainCameraTransform = cameraC.GetGameObject().transform;
            mekaControllerC.mekaTransform = mekaC.GetGameObject().transform;

            scene.AddGameObject(go);

            return scene;

        }

    }
}
