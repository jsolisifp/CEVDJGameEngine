using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Silk.NET.Assimp;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace GameEngine
{

    internal class MekaPlayerController : Component, IMekaController
    {

        
        public Transform mekaTransform;
        public Transform mainCameraTransform;
        public Transform targetingZone;

        public Vector3 cameraOffset;
        public float centeringSpeed;

        public Vector2 xCameraBoundaries;
        public Vector2 yCameraBoundaries;
        public Vector2 zCameraBoundaries;


        public Vector3 targetingOffset;

        Meka meka;
        Target mekaTarget;

        Target currentTarget;
        List<Target> targets;
        
        public MekaPlayerController()
        {
            targetingOffset= new Vector3(0,0,-22);
            cameraOffset = new Vector3(0, 2.5f, -5);
            
            centeringSpeed = 7;
            xCameraBoundaries = new Vector2(4,-4);
            yCameraBoundaries = new Vector2(5,-5);
            zCameraBoundaries = new Vector2(-3,-10);

        }
        public override void Start()
        {
            meka = mekaTransform.GetGameObject().GetComponent<Meka>();
            mekaTarget = meka.hitBox.GetGameObject().GetComponent<Target>();
            targets = new List<Target>();
            Editor.SetPlayerController(this);
        }


        public override void Update(float deltaTime)
        {
            if(mekaTransform == null || meka == null || mainCameraTransform == null || targetingZone == null) return;

            CenterCamera(deltaTime);
            CameraRotation();
            TargetingSystem();
            ControlMeka(deltaTime);

        }

        private void CenterCamera(float deltaTime)
        {

            Vector3 position = mekaTransform.InverseTransformPosition(mainCameraTransform.position);

            if (position.X != cameraOffset.X)
            {
                if (position.X < cameraOffset.X)
                {
                    position.X += centeringSpeed * deltaTime;
                    if (position.X > cameraOffset.X) position.X = cameraOffset.X;
                    else if (position.X < xCameraBoundaries.Y) position.X = xCameraBoundaries.Y;
                }
                else
                {
                    position.X -= centeringSpeed * deltaTime;
                    if (position.X < cameraOffset.X) position.X = cameraOffset.X;
                    else if (position.X > xCameraBoundaries.X) position.X = xCameraBoundaries.X;
                }
            }

            if (position.Y != cameraOffset.Y)
            {
                if (position.Y < cameraOffset.Y)
                {
                    position.Y += centeringSpeed * deltaTime;
                    if (position.Y > cameraOffset.Y) position.Y = cameraOffset.Y;
                    else if (position.Y < yCameraBoundaries.Y) position.Y = yCameraBoundaries.Y;
                }
                else
                {
                    position.Y -= centeringSpeed * deltaTime;
                    if (position.Y < cameraOffset.Y) position.Y = cameraOffset.Y;
                    else if (position.Y > yCameraBoundaries.X) position.Y = yCameraBoundaries.X;
                }
            }

            if (position.Z != cameraOffset.Z)
            {
                if (position.Z < cameraOffset.Z)
                {
                    position.Z += centeringSpeed * deltaTime;
                    if (position.Z > cameraOffset.Z) position.Z = cameraOffset.Z;
                    else if (position.Z < zCameraBoundaries.Y) position.Z = zCameraBoundaries.Y;
                }
                else
                {
                    position.Z -= centeringSpeed * deltaTime;
                    if (position.Z < cameraOffset.Z) position.Z = cameraOffset.Z;
                    else if (position.Z > zCameraBoundaries.X) position.Z = zCameraBoundaries.X;
                }
            }

            mainCameraTransform.position = mekaTransform.TransformPosition(position);
        }

        private void CameraRotation()
        {
            if (!meka.isAiming || !meka.isTargetLock)
            {
                mainCameraTransform.rotation.X = 0;
                mainCameraTransform.rotation.Y = mekaTransform.rotation.Y + 180;
                mainCameraTransform.rotation.Z = 0;
            } else {
                mainCameraTransform.LookAt(meka.currentAim, Vector3.UnitY);
            }

        }
         
        Vector3 input;
        float rotation;
        float lastTargetChange;
        float uLastPressed;
        float iLastPressed;
        float ctrlLastPressed;
        private void ControlMeka(float deltaTime)
        {

            if (Input.IsKeyPressed(Key.W)) { input.Z = 1; }
            else if (Input.IsKeyPressed(Key.S)) { input.Z = -1; }
            else { input.Z = 0; }

            if (Input.IsKeyPressed(Key.A)) { input.X = 1; }
            else if (Input.IsKeyPressed(Key.D)) { input.X = -1; }
            else { input.X = 0;                             }

            if (Input.IsKeyPressed(Key.Space)) { input.Y = 1; }
            else { input.Y = 0; }

            if (meka.isAiming && meka.isTargetLock)
            {
                rotation = 0;
                if (Input.IsKeyPressed(Key.Q) && lastTargetChange > 0.4) { ChangeTarget(-1); lastTargetChange = 0; }
                else if (Input.IsKeyPressed(Key.E) && lastTargetChange > 0.4) { ChangeTarget(1); lastTargetChange = 0; }
                else lastTargetChange += deltaTime; 
            }
            else
            {
                if (Input.IsKeyPressed(Key.Q)) { rotation = 1; }
                else if (Input.IsKeyPressed(Key.E)) { rotation = -1; }
                else { rotation = 0; }
            }

            if (Input.IsKeyPressed(Key.J)) meka.Shoot(0);
            if (Input.IsKeyPressed(Key.K)) meka.Shoot(1);

            if (Input.IsKeyPressed(Key.U) && uLastPressed > 0.4) { meka.SwapWeapon(0); uLastPressed = 0; }
            else uLastPressed += deltaTime;
            if (Input.IsKeyPressed(Key.I) && iLastPressed > 0.4) { meka.SwapWeapon(1); iLastPressed = 0; }
            else iLastPressed += deltaTime;

            if (Input.IsKeyPressed(Key.ControlLeft) && ctrlLastPressed > 0.4) { meka.isTargetLock = !meka.isTargetLock; ctrlLastPressed = 0; }
            else ctrlLastPressed += deltaTime;

            meka.InputMovement(input, rotation, currentTarget);
        }

        private void ChangeTarget(int change)
        {
            if (targets.Count <= 1 || currentTarget == null) return;
            int num = targets.IndexOf(currentTarget) + change;
            if (num < 0) num = targets.Count-1;
            else if (num >= targets.Count) num = 0;

            currentTarget = targets[num];

        }

        private void TargetingSystem()
        {
            targetingZone.rotation = mekaTransform.rotation;
            targetingZone.position = mainCameraTransform.TransformPosition(targetingOffset);

            if (targets.Count == 0) { currentTarget = null; return; }

            if (meka.isTargetLock && currentTarget != null && currentTarget.teamId != -1) return;
            meka.isTargetLock = false;
            Target tmpTarget = null;
            Vector3 tmpVector;
            float distance = -1;
            for (int i = 0; i < targets.Count; i++)
            {
                tmpVector = mekaTransform.InverseTransformPosition(targets[i].GetGameObject().transform.position);
                if(i == 0 || tmpVector.Length()<distance)
                { 
                    tmpTarget = targets[i];
                    distance = tmpVector.Length();
                }
            }
            currentTarget = tmpTarget;
        }
        public void AddTarget(Target target)
        {
            if (target.teamId == -1 || target.teamId == mekaTarget.teamId) return;
            targets.Add(target);
        }

        public void RemoveTarget(Target target)
        {
            targets.Remove(target);
        }

        public void DrawHud(IWindow window)
        {
            ImDrawListPtr drawListPtr = ImGui.GetBackgroundDrawList();
            
            Vector2 windowSize = new Vector2(window.Size.X, window.Size.Y);
            //Console.WriteLine("WindowSize1 "+windowSize);
            uint hudColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1));
            uint transparentColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.8f, 0.8f, 0.8f, 0.25f));
            uint hudBackColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 0.5f, 1));
            uint lockOnColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.8f, 0.8f, 0.8f, 0.75f));

            //HP START
            drawListPtr.AddRectFilled(new Vector2(0, windowSize.Y), new Vector2(20 + windowSize.X / 8, windowSize.Y - 70), transparentColor, 0.25f);

            int hp = meka.hp;
            int maxHp = meka.maxHp;
            string hpString = hp + "";
            int dif = 4 - hpString.Length;
            for (int i = 0; i < dif; i++)
            {
                hpString = "0" + hpString;
            }
            drawListPtr.AddText(ImGui.GetFont(), 30f, new Vector2(10, windowSize.Y - 60), hudColor, "HP");
            drawListPtr.AddText(ImGui.GetFont(), 40f, new Vector2(windowSize.X / 8 - 75, windowSize.Y - 70), hudColor, hpString);

            drawListPtr.AddRectFilled(new Vector2(10, windowSize.Y - 10), new Vector2(10 + windowSize.X / 8, windowSize.Y - 30), hudBackColor);
            drawListPtr.AddRectFilled(new Vector2(10, windowSize.Y - 10), new Vector2((10 + windowSize.X / 8) * hp / maxHp, windowSize.Y - 30), hudColor);
            //HP END

            //AMMO START
            drawListPtr.AddRectFilled(new Vector2(windowSize.X, windowSize.Y), new Vector2(windowSize.X - 20 - windowSize.X / 8, windowSize.Y - 70), transparentColor, 0.25f);
            Weapon weapon = meka.leftWeapon;
            
            int ammo = weapon.ammo;
            int maxAmmo = weapon.maxAmmo;
            string ammoString = ammo + "";
            dif = 3 - ammoString.Length;
            for (int i = 0; i < dif; i++)
            {
                ammoString = "0" + ammoString;
            }
            float percent = weapon.isReloading ? weapon.ReloadPercent() : 1f*ammo/maxAmmo;

            drawListPtr.AddText(ImGui.GetFont(), 25, new Vector2(windowSize.X - 10 - windowSize.X / 8, windowSize.Y - 67), hudColor, ammoString);
            drawListPtr.AddText(ImGui.GetFont(), 22, new Vector2(windowSize.X - 72, windowSize.Y - 65), hudColor, "Left");

            drawListPtr.AddRectFilled(new Vector2(windowSize.X - 10, windowSize.Y - 38), new Vector2(windowSize.X - 10 - windowSize.X / 8, windowSize.Y - 43), hudBackColor);
            drawListPtr.AddRectFilled(new Vector2(windowSize.X - 10, windowSize.Y - 38), new Vector2(windowSize.X + (-10 - windowSize.X / 8) * percent, windowSize.Y - 43), hudColor);

            weapon =meka.rightWeapon;

            ammo = weapon.ammo;
            maxAmmo = weapon.maxAmmo;
            ammoString = ammo + "";
            dif = 3 - ammoString.Length;
            for (int i = 0; i < dif; i++)
            {
                ammoString = "0" + ammoString;
            }
            percent = weapon.isReloading ? weapon.ReloadPercent() : 1f * ammo / maxAmmo;

            drawListPtr.AddText(ImGui.GetFont(), 25, new Vector2(windowSize.X - 10 - windowSize.X / 8, windowSize.Y - 38), hudColor, ammoString);
            drawListPtr.AddText(ImGui.GetFont(), 22, new Vector2(windowSize.X - 72, windowSize.Y - 36), hudColor, "Right");

            drawListPtr.AddRectFilled(new Vector2(windowSize.X - 10, windowSize.Y - 9), new Vector2(windowSize.X - 10 - windowSize.X / 8, windowSize.Y - 14), hudBackColor);
            drawListPtr.AddRectFilled(new Vector2(windowSize.X - 10, windowSize.Y - 9), new Vector2(windowSize.X + (-10 - windowSize.X / 8) * percent, windowSize.Y - 14), hudColor);
            //AMMO END

            //CROSSHAIR START
            Vector2 crosshair = windowSize * 0.5f;
            Vector3 aim = mainCameraTransform.InverseTransformPosition(meka.currentAim);
            float radius = windowSize.X/15 + -aim.Length()*3;
            crosshair.X -= crosshair.X * aim.X / aim.Z;
            crosshair.Y += crosshair.Y * 1.75f * aim.Y / aim.Z ;

            drawListPtr.AddCircle(crosshair, radius, lockOnColor, 6, 5);

            //drawListPtr.AddLine(new(crosshair.X,0),new(crosshair.X,windowSize.Y),lockOnColor);
            //drawListPtr.AddLine(new(0,crosshair.Y),new(windowSize.X,crosshair.Y),lockOnColor);

            Console.WriteLine("Aim=" + aim);
            Console.WriteLine("Crosshair=" + crosshair.X);

            //CROSSHAIR END
        }
    }
}
