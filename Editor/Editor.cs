using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

namespace GameEngine
{
    internal class Editor
    {
        const int selectedGameObjectsListInitialCapacity = 100;
        const int maxNameLength = 20;
        const ImGuiWindowFlags defaultWindowFlags = ImGuiWindowFlags.AlwaysAutoResize;

        static ImGuiController controller;
        static IWindow window;

        static bool enabled;
        static bool sceneViewEnabled;
        static bool gameObjectViewEnabled;
        static bool assetsViewEnabled;
        static bool collidersViewEnabled;

        static HashSet<GameObject> selectedGameObjectsSet;
        static List<GameObject> selectedGameObjectsList;
        static string selectedAssetId;


        static bool modalAboutOpened;
        static bool modalSaveSceneAs;
        static bool modalAlertOpened;
        static bool modalPickTransformOpened;

        static bool openSaveSceneModal;
        static bool openAboutModal;
        static bool openAlertModal;
        static bool openPickTransformModal;

        static string modalFilenameText;
        static string modalAlertText;

        static Component modalPickTransformTargetComponent;
        static FieldInfo modalPickTransformTargetField;

        static Vector3 cameraPosition;
        static Vector3 cameraRotation;
        static float cameraFov;
        static float cameraZNear;
        static float cameraZFar;

        static Vector2 cameraMousePreviousPosition;
        static bool editorView;

        public static void Init(IWindow _window, IInputContext _input, GL _gl)
        {
            controller = new ImGuiController(_gl, _window, _input);
            window = _window;

            Input.onKeyDown += OnKeyDown;
            Input.onMouseMove += OnMouseMove;
            Render.onOverrideView += OnOverrideView;
            Render.onRenderOverlay += OnRender;
            
            enabled = true;
            Input.SetCursorVisible(true);

            collidersViewEnabled = true;
            Physics.SetRenderCollidersEnabled(true);

            sceneViewEnabled = true;
            gameObjectViewEnabled = true;
            assetsViewEnabled = true;

            selectedAssetId = null;

            selectedGameObjectsSet = new HashSet<GameObject>(selectedGameObjectsListInitialCapacity);
            selectedGameObjectsList = new List<GameObject>(selectedGameObjectsListInitialCapacity);

            modalAboutOpened = true;
            modalSaveSceneAs = true;
            modalAlertOpened = true;
            modalPickTransformOpened = true;

            cameraPosition = new Vector3(0, 5, -10);
            cameraRotation = new Vector3(0, 180, 0);
            cameraFov = 60;
            cameraZNear = 0.1f;
            cameraZFar = 1000.0f;

            editorView = true;

            cameraMousePreviousPosition = Input.GetMousePosition();


        }

        public static void Update(float deltaTime)
        {
            if(!enabled) { return; }

            // Update camera

            if(Input.IsMouseButtonPressed(1) && editorView)
            {
                Vector3 cameraForward = TransformCameraDirection(Vector3.UnitZ);
                Vector3 cameraRight = TransformCameraDirection(Vector3.UnitX);
                Vector3 cameraUp = TransformCameraDirection(Vector3.UnitY);
                bool boost = Input.IsKeyPressed(Key.ShiftLeft) || Input.IsKeyPressed(Key.ShiftRight);

                if (Input.IsKeyPressed(Key.W)) { cameraPosition -= (boost ? 20 : 10) * cameraForward * deltaTime; }
                else if (Input.IsKeyPressed(Key.S)) { cameraPosition += (boost ? 20 : 10) * cameraForward * deltaTime; }
                if (Input.IsKeyPressed(Key.A)) { cameraPosition -= (boost ? 20 : 10) * cameraRight * deltaTime; }
                else if (Input.IsKeyPressed(Key.D)) { cameraPosition += (boost ? 20 : 10) * cameraRight * deltaTime; }
                if (Input.IsKeyPressed(Key.Q)) { cameraPosition -= (boost ? 20 : 10) * cameraUp * deltaTime; }
                else if (Input.IsKeyPressed(Key.E)) { cameraPosition += (boost ? 20 : 10) * cameraUp * deltaTime; }
            }

            cameraMousePreviousPosition = Input.GetMousePosition();

            controller.Update(deltaTime);
        }

        public static void Finish()
        {
            Render.onRenderOverlay -= OnRender;
            Render.onOverrideView -= OnOverrideView;
            Input.onKeyDown -= OnKeyDown;
            Input.onMouseMove -= OnMouseMove;


            controller.Dispose();
        }

        static void SwitchEnabled()
        {
            enabled = !enabled;

            if(enabled)
            {
                if(Engine.GetState() == Engine.State.playing)
                {
                    Engine.Pause();
                }
            }

            Input.SetCursorVisible(enabled);

        }

        public static void OnRender(float deltaTime)
        {
            if(!enabled) { return; }

            DrawMenu();
            DrawViews();
            DrawModals();

            controller.Render();
        }

        private static void OnOverrideView(ref Matrix4x4 viewMatrix, ref Matrix4x4 projectionMatrix)
        {
            if(!editorView) { return; }

            float zRads = MathUtils.DegreesToRadians(cameraRotation.Z);
            float yRads = MathUtils.DegreesToRadians(cameraRotation.Y);
            float xRads = MathUtils.DegreesToRadians(cameraRotation.X);

            float fovRads = MathUtils.DegreesToRadians(cameraFov);

            Vector2D<int> size = window.FramebufferSize;

            Matrix4x4 temp = Matrix4x4.CreateRotationX(xRads) *
                            Matrix4x4.CreateRotationY(yRads) *
                            Matrix4x4.CreateRotationZ(zRads) *
                            Matrix4x4.CreateTranslation(cameraPosition);
            Matrix4x4.Invert(temp, out viewMatrix);
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRads, (float)size.X / size.Y, cameraZNear, cameraZFar);

        }

        static void OnMouseMove(Vector2 position)
        {
            if(Input.IsMouseButtonPressed(1))
            {
                Vector2 displacement = position - cameraMousePreviousPosition;

                cameraRotation.Y -= displacement.X / (float)window.Size.X * 360.0f;
                cameraRotation.X -= displacement.Y / (float)window.Size.Y * 180.0f;

            }

            cameraMousePreviousPosition = position;
        }

        static void OnKeyDown(Key key, int arg3)
        {
            if(key == Key.F1)
            {
                SwitchEnabled();
            }

        }

        static void DrawMenu()
        {
            Engine.State state = Engine.GetState();
            bool isEngineStopped = state == Engine.State.stopped;
            bool isEnginePaused = state == Engine.State.paused;
            bool isEnginePlaying = state == Engine.State.playing;


            if (ImGui.BeginPopupModal("Pick transform", ref modalPickTransformOpened, defaultWindowFlags))
            {
                Transform picked = null;
                bool done = false;
                List<GameObject> gameObjects = SceneManager.GetActiveScene().GetGameObjects();

                if(ImGui.Selectable("none")) { done = true; picked = null; }

                for(int i = 0; i < gameObjects.Count; i ++)
                {
                    GameObject go = gameObjects[i];
                    if(ImGui.Selectable(go.name)) { done = true; picked = go.transform; }
                }

                if(done)
                {
                    modalPickTransformTargetField.SetValue(modalPickTransformTargetComponent, picked);
                    modalPickTransformTargetField = null;
                    modalPickTransformTargetComponent = null;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("About", ref modalAboutOpened, defaultWindowFlags))
            {
                ImGui.Text("My game engine");
                ImGui.Text("License CC BY-NC-SA");

                if (ImGui.Button("Accept")) { ImGui.CloseCurrentPopup(); }

                ImGui.EndPopup();
            }

            // Not used now, but tested
            //if(ImGui.BeginPopupModal("Alert", ref modalAlertOpened, defaultWindowFlags))
            //{
            //    ImGui.Text(modalAlertText);

            //    if (ImGui.Button("Accept")) { ImGui.CloseCurrentPopup(); }

            //    ImGui.EndPopup();
            //}

            if (ImGui.BeginPopupModal("Save scene as", ref modalSaveSceneAs, defaultWindowFlags))
            {
                ImGui.InputText("Filename", ref modalFilenameText, maxNameLength);
                if (ImGui.Button("Accept"))
                {
                    string id = modalFilenameText.Trim();
                    if(!id.EndsWith(".scene")) { id += ".scene"; }
                    Scene scene = SceneManager.GetActiveScene();

                    SceneSerializer.Serialize(scene, Assets.GetAssetsPath() + "\\" + id);

                    if(!Assets.IsAssetLoaded(id))
                    { Assets.LoadAsset(id); }
                    else  { Assets.ReloadAsset(id); }

                    SceneManager.SetActiveSceneAssetId(id);


                    ImGui.CloseCurrentPopup();
                }

                if(ImGui.Button("Cancel"))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            ///////////////// Menu bar ////////////////////

            ImGui.BeginMainMenuBar();

            if (ImGui.BeginMenu("Scene"))
            {
                if(ImGui.MenuItem("New scene", isEngineStopped))
                {
                    SceneManager.New();
                    selectedGameObjectsSet.Clear();
                    selectedGameObjectsList.Clear();
                }

                bool isSceneSelected = false;
                if (selectedAssetId != null)
                {   if (Assets.GetLoadedAssetType(selectedAssetId, false) == typeof(Scene))
                    { isSceneSelected = true;  }
                }

                if(ImGui.MenuItem("Load scene", isSceneSelected && isEngineStopped))
                {
                    LoadSelectedScene();
                    selectedGameObjectsSet.Clear();
                    selectedGameObjectsList.Clear();
                }

                if (ImGui.MenuItem("Save scene", isEngineStopped))
                {

                    Scene scene = SceneManager.GetActiveScene();
                    string id = SceneManager.GetActiveSceneAssetId();

                    if(id == null)
                    {
                        openSaveSceneModal = true;
                    }
                    else
                    {
                        SceneSerializer.Serialize(scene, Assets.GetAssetsPath() + "\\" + id);
                        Assets.ReloadAsset(id);
                    }

                }
                else if(ImGui.MenuItem("Save scene as...", isEngineStopped))
                {
                    openSaveSceneModal = true;
                }

                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Playback"))
            {
               

                if(ImGui.MenuItem("Play", isEngineStopped))
                {
                    Engine.Play();
                    editorView = false; 
                }

                if(ImGui.MenuItem("Pause", isEnginePlaying))
                {
                    Engine.Pause();
                    editorView = true;
                }

                if (ImGui.MenuItem("Resume", isEnginePaused))
                {
                    Engine.Resume();
                    editorView = false;
                }

                if (ImGui.MenuItem("Stop", isEnginePlaying || isEnginePaused))
                {
                    Engine.Stop();
                    editorView = true;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Game Object"))
            {
                if (ImGui.MenuItem("Create", ""))
                {
                    GameObject go = new GameObject();
                    go.name = "New game object";

                    Transform t = new Transform();
                    go.AddComponent(t);
                    t.Start();

                    Scene s = SceneManager.GetActiveScene();
                    s.AddGameObject(go);
                }

                if (ImGui.BeginMenu("Add component", selectedGameObjectsList.Count == 1))
                {
                    Assembly a = Assembly.GetAssembly(typeof(Editor));
                    Type[] types = a.GetTypes();

                    for (int i = 0; i < types.Length; i++)
                    {
                        Type t = types[i];

                        if (t.IsSubclassOf(typeof(Component)))
                        {
                            if(ImGui.MenuItem(t.Name))
                            {
                                object o = Activator.CreateInstance(t);
                                Component c = (Component)o;

                                if(selectedGameObjectsList.Count == 1)
                                {
                                    selectedGameObjectsList[0].AddComponent(c);
                                }
                            }

                        }
                    }

                    ImGui.EndMenu();
                }

                if(ImGui.BeginMenu("Remove component", selectedGameObjectsList.Count == 1))
                {
                    if(selectedGameObjectsList.Count == 1)
                    {
                        List<Component> components = selectedGameObjectsList[0].GetComponents();

                        for(int i = 0; i < components.Count; i ++)
                        {
                            Component c = components[i];
                            Type t = c.GetType();

                            if(t.Name != "Transform")
                            {
                                if(ImGui.MenuItem(t.Name))
                                {
                                    c.Stop();
                                    selectedGameObjectsList[0].RemoveComponent(c);
                                }
                            }
                        }
                    }

                    ImGui.EndMenu();
                }


                if(ImGui.MenuItem("Delete", selectedGameObjectsList.Count == 1))
                {
                    if(selectedGameObjectsList.Count == 1)
                    {
                        RemoveSelectedGameObject();
                    }
                }


                ImGui.EndMenu();
            }


            if(ImGui.BeginMenu("Assets"))
            {
                if(ImGui.MenuItem("Reload all"))
                {
                    Assets.ReloadAssets();
                }

                if(ImGui.MenuItem("Reload", selectedAssetId != null))
                {
                    if(selectedAssetId != null)
                    {
                        Assets.ReloadAsset(selectedAssetId, false);
                    }
                }

                if(ImGui.MenuItem("Delete", selectedAssetId != null))
                {
                    if(selectedAssetId != null)
                    {
                        Assets.UnloadAsset(selectedAssetId, false);

                        System.IO.File.Delete(selectedAssetId);

                        selectedAssetId = "";
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Scene", ""))
                {
                    sceneViewEnabled = !sceneViewEnabled;
                }
                
                if (ImGui.MenuItem("Game object", ""))
                {
                    gameObjectViewEnabled = !gameObjectViewEnabled;
                }

                if (ImGui.MenuItem("Assets", ""))
                {
                    assetsViewEnabled = !assetsViewEnabled;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Colliders", "", collidersViewEnabled))
                {
                    collidersViewEnabled = !collidersViewEnabled;
                    Physics.SetRenderCollidersEnabled(collidersViewEnabled);
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Game camera", "", !editorView))
                {
                    editorView = false;
                }

                if (ImGui.MenuItem("Editor camera", "", editorView))
                {
                    editorView = true;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("About", ""))
                {
                    openAboutModal = true;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();

        }

        static void DrawModals()
        {
            //////////////// Modals ///////////////////

            if (openPickTransformModal)
            {
                ImGui.OpenPopup("Pick transform");
            }

            if (openAboutModal)
            {
                ImGui.OpenPopup("About");
            }

            if (openSaveSceneModal)
            {
                Scene scene = SceneManager.GetActiveScene();
                modalFilenameText = scene.name;
                ImGui.OpenPopup("Save scene as");
            }

            if(openAlertModal)
            {
                ImGui.OpenPopup("Alert");
            }

            openSaveSceneModal = false;
            openAboutModal = false;
            openAlertModal = false;
            openPickTransformModal = false;


        }

        static void DrawViews()
        {

            if(sceneViewEnabled)
            {
                DrawSceneView();
            }

            if (gameObjectViewEnabled)
            {
                DrawGameObjectView();

            }

            if(assetsViewEnabled)
            {
                DrawAssetsView();
            }
        }

        static void DrawSceneView()
        {
            ImGui.Begin("Scene view", defaultWindowFlags);

            Scene scene = SceneManager.GetActiveScene();

            ImGui.InputText("Name", ref scene.name, maxNameLength);

            if (ImGui.CollapsingHeader("Game Objects"))
            {
                List<GameObject> gameObjects = scene.GetGameObjects();

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    GameObject go = gameObjects[i];

                    if(ImGui.Selectable(go.name, selectedGameObjectsSet.Contains(go)))
                    {
                        if (!ImGui.GetIO().KeyCtrl)
                        {
                            selectedGameObjectsSet.Clear();
                            selectedGameObjectsList.Clear();
                        }

                        if (selectedGameObjectsSet.Contains(go))
                        {
                            selectedGameObjectsList.Remove(go);
                            selectedGameObjectsSet.Remove(go);
                        }
                        else
                        {
                            selectedGameObjectsList.Add(go);
                            selectedGameObjectsSet.Add(go);

                        }
                    }
                }

            }

            ImGui.End();

        }

        static void DrawGameObjectView()
        {
            ImGui.Begin("Game object view", defaultWindowFlags);


            if (selectedGameObjectsList.Count == 0)
            {
                ImGui.Text("No game object selected");
                ImGui.End();
                return;
            }
            else if (selectedGameObjectsList.Count > 1)
            {
                ImGui.Text(String.Format("Multiple objects selected ({0:0})", selectedGameObjectsList.Count));
                ImGui.End();
                return;
            }


            GameObject go = selectedGameObjectsList[0];

            ImGui.InputText("name", ref go.name, maxNameLength);
            ImGui.Checkbox("active", ref go.active);
            ImGui.Checkbox("static", ref go.@static);
            ImGui.InputInt("layer", ref go.layer);

            List<Component> components = go.GetComponents();

            for (int i = 0; i < components.Count; i++)
            {
                Component c = components[i];
                Type t = c.GetType();
                if (ImGui.CollapsingHeader(t.Name))
                {
                    ImGui.Checkbox("active", ref c.active);

                    FieldInfo[] fields = t.GetFields();

                    for (int j = 0; j < fields.Length; j++)
                    {
                        FieldInfo f = fields[j];
                        object value = f.GetValue(c);
                        Type type = f.FieldType;

                        if (type.Name == "String")
                        {
                            string s = (string)value;
                            if (s == null) { s = ""; }
                            if (ImGui.InputText(f.Name, ref s, maxNameLength))
                            {
                                f.SetValue(c, s);
                            }
                        }
                        else if (type.Name == "Int32")
                        {
                            int n = (int)value;

                            if (ImGui.InputInt(f.Name, ref n))
                            {
                                f.SetValue(c, n);
                            }
                        }
                        else if (type.Name == "Single")
                        {
                            Single s = (Single)value;

                            if (ImGui.InputFloat(f.Name, ref s))
                            {
                                f.SetValue(c, s);
                            }
                        }
                        else if (type.Name == "Boolean" && f.Name != "active")
                        {
                            bool b = (bool)value;

                            if (ImGui.Checkbox(f.Name, ref b))
                            {
                                f.SetValue(c, b);
                            }
                        }
                        else if (type.Name == "Vector3")
                        {
                            Vector3 v = (Vector3)value;

                            if (ImGui.InputFloat3(f.Name, ref v))
                            {
                                f.SetValue(c, v);
                            }
                        }
                        else if (type.Name == "Vector4")
                        {
                            Vector4 v = (Vector4)value;

                            if (ImGui.InputFloat4(f.Name, ref v))
                            {
                                f.SetValue(c, v);
                            }
                        }
                        else if (type.Name == "Transform")
                        {
                            Transform t2 = (Transform)value;
                            string text = (t2 != null ? t2.GetGameObject().name : "none");
                            ImGui.InputText(f.Name, ref text, maxNameLength, ImGuiInputTextFlags.ReadOnly); ImGui.SameLine();
                            if (ImGui.Button("Pick"))
                            {
                                modalPickTransformTargetField = f;
                                modalPickTransformTargetComponent = c;
                                openPickTransformModal = true;
                            }
                        }

                    }

                }
            }

            ImGui.End();

        }

        static void DrawAssetsView()
        {
            ImGui.Begin("Assets view", defaultWindowFlags);

            string assetsPath = Assets.GetAssetsPath();

            List<string> paths = Assets.GetAssetPaths();

            for(int i = 0; i < paths.Count; i++)
            {
                string path = paths[i];

                bool isSceneAsset = (Assets.GetLoadedAssetType(path, false) == typeof(Scene));
    
                if(isSceneAsset)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 1)));
                }

                if(ImGui.Selectable(paths[i].Substring(assetsPath.Length + 1), paths[i] == selectedAssetId, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    selectedAssetId = paths[i];

                    if(ImGui.IsMouseDoubleClicked(0))
                    {
                        if(Engine.GetState() != Engine.State.stopped) { Engine.Stop(); }

                        if (isSceneAsset) { LoadSelectedScene(); }
                        else { ExplorerOpenSelectedAsset();  }

                    }
                }

                if (isSceneAsset)
                {
                    ImGui.PopStyleColor();
                }
            }

            ImGui.End();

        }

        static void RemoveSelectedGameObject()
        {
            selectedGameObjectsList[0].Stop();

            // Remove references to game object

            Scene activeScene = SceneManager.GetActiveScene();
            List<GameObject> gos = activeScene.GetGameObjects();
            for(int i = 0; i < gos.Count; i ++)
            {
                GameObject go = gos[i];
                List<Component> cs = go.GetComponents();
                for(int j = 0; j < cs.Count; j++)
                {
                    Component c = cs[j];
                    Type t = c.GetType();
                    FieldInfo[] fs = t.GetFields();
                    for(int k = 0; k < fs.Length; k ++)
                    {
                        FieldInfo f = fs[k];
                        Type ft = f.FieldType;

                        if(ft.Name == "Transform")
                        {
                            object v = f.GetValue(c);
                            if(v == selectedGameObjectsList[0].transform)
                            {
                                f.SetValue(c, null);
                            }
                        }
                    }
                }
            }

            activeScene.RemoveGameObject(selectedGameObjectsList[0]);

            selectedGameObjectsList.Clear();

        }

        static void LoadSelectedScene()
        {
            string relativeId = Assets.ToRelativePath(selectedAssetId);
            SceneManager.SetActiveSceneAssetId(relativeId);
        }

        static void ExplorerOpenSelectedAsset()
        {
            var info = new ProcessStartInfo();
            info.FileName = "explorer";
            info.Arguments = "\"" + selectedAssetId + "\"";
            Process.Start(info);
        }

        static Vector3 TransformCameraDirection(Vector3 direction)
        {
            float rotX = MathUtils.DegreesToRadians(cameraRotation.X);
            float rotY = MathUtils.DegreesToRadians(cameraRotation.Y);
            float rotZ = MathUtils.DegreesToRadians(cameraRotation.Z);

            Matrix4x4 model =
                                Matrix4x4.CreateRotationX(rotX) *
                                Matrix4x4.CreateRotationY(rotY) *
                                Matrix4x4.CreateRotationZ(rotZ) *
                                Matrix4x4.CreateTranslation(cameraPosition);

            Vector4 direction4 = new Vector4(direction, 0);
            Vector4 r = Vector4.Transform(direction4, model);

            return new Vector3(r.X, r.Y, r.Z);

        }

    }
}
