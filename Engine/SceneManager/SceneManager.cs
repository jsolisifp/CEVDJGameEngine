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

            PresetLoader presetLoader = new PresetLoader();
            Assets.RegisterAssetLoader("preset", presetLoader);
            Assets.RegisterAssetLoader("component", presetLoader);

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
            MekaPlayerController mekaControllerC;
            Target targetC;
            TargetingZone targetingZoneC;
            SimpleController simpleControllerC;
            Projectile projectileC;

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
            boxC.size = new Vector3(1, 1, 1);

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
            go.name = "Column";
            go.@static = true;
            go.AddComponent(new Transform());

            boxC = new BoxCollider();
            boxC.size = new Vector3(1, 1, 1);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();

            go.AddComponent(rigidC);

            go.transform.position = new Vector3(3, 4, 3);
            go.transform.scale = new Vector3(2, 8, 2);

            rendererC = new Renderer();
            rendererC.modelId = "UnitBox.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Rock.png";

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
            go.name = "MekaHitbox";
            go.AddComponent(new Transform());
            mekaC.hitBox = go.transform;
            boxC = new BoxCollider();
            boxC.size = new Vector3(1, 1, 1);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;

            go.AddComponent(rigidC);

            targetC = new Target();

            targetC.mekaTransform = mekaC.GetGameObject().transform;

            go.AddComponent(targetC);

            scene.AddGameObject (go);

            go = new GameObject();
            go.name = "MekaController";
            go.AddComponent(new Transform());
            mekaControllerC = new MekaPlayerController();
            go.AddComponent(mekaControllerC);
            mekaControllerC.mainCameraTransform = cameraC.GetGameObject().transform;
            mekaControllerC.mekaTransform = mekaC.GetGameObject().transform;

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "TargetingZone";
            go.AddComponent(new Transform());
            
            boxC = new BoxCollider();
            boxC.size = new Vector3(20, 20, 30);

            go.AddComponent(boxC);

            triggerC = new Trigger();

            go.AddComponent(triggerC);
            
            targetingZoneC = new TargetingZone();
            targetingZoneC.controller = mekaControllerC.GetGameObject().transform;
            mekaControllerC.targetingZone = go.transform;
            go.AddComponent (targetingZoneC);

            scene.AddGameObject(go);


            go = new GameObject();
            go.name = "MekaDummy";
            go.AddComponent(new Transform());
            go.transform.position = new Vector3 (-5, 5, 5);
            go.transform.rotation = new Vector3(0, 180, 0);
            mekaC = new Meka();
            go.AddComponent(mekaC);
            weaponC = new Weapon();
            go.AddComponent(weaponC);
            mekaC.leftWeapon = weaponC;
            weaponC = new Weapon();
            go.AddComponent(weaponC);
            mekaC.rightWeapon = weaponC;
            mekaC.textures[4] = "Red.png";
            simpleControllerC = new SimpleController();
            simpleControllerC.mekaTransform=go.transform;
            simpleControllerC.input=-Vector3.UnitZ;
            simpleControllerC.rotation = 1;
            go.AddComponent(simpleControllerC);

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "MekaDummyHitbox";
            go.AddComponent(new Transform());
            mekaC.hitBox = go.transform;
            boxC = new BoxCollider();
            boxC.size = new Vector3(1, 1, 1);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;

            go.AddComponent(rigidC);

            targetC = new Target();
            targetC.teamId = 1;
            targetC.mekaTransform = mekaC.GetGameObject().transform;

            go.AddComponent(targetC);

            scene.AddGameObject(go);

            go = new GameObject();
            go.name = "Proyectil";
            go.AddComponent(new Transform());

            rendererC = new Renderer();
            rendererC.modelId = "UnitBox.obj";
            rendererC.shaderId = "Default.shader";
            rendererC.textureId = "Yellow.png";

            go.AddComponent(rendererC);

            boxC = new BoxCollider();
            boxC.size = new Vector3(1, 1, 1);

            go.AddComponent(boxC);

            rigidC = new Rigidbody();
            rigidC.isKinematic = true;

            go.AddComponent(rigidC);

            projectileC = new Projectile();
            projectileC.teamId = 1;
            projectileC.speed = 1;

            go.AddComponent(projectileC);

            scene.AddGameObject(go);

            return scene;

        }

    }
}
