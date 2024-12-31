using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Assimp;
using Silk.NET.Input;

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
        float fLastPressed;
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

            if (Input.IsKeyPressed(Key.Q)) { rotation = 1; }
            else if (Input.IsKeyPressed(Key.E)) { rotation = -1; }
            else { rotation = 0; }

            if (Input.IsKeyPressed(Key.F) && fLastPressed > 0.5) { meka.isAiming = !meka.isAiming; fLastPressed = 0; }
            else fLastPressed += deltaTime; 

            if (Input.IsKeyPressed(Key.ControlLeft) && ctrlLastPressed > 0.5) { meka.isTargetLock = !meka.isTargetLock; ctrlLastPressed = 0; }
            else ctrlLastPressed += deltaTime;


            meka.InputMeka(input, rotation, currentTarget);

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
    }
}
