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
    internal class MekaController : Component
    {
        public Transform mekaTransform;
        public Transform mainCameraTransform;

        public Vector3 cameraOffset;
        public float centeringSpeed;
        public float cameraSpeed;

        public Vector2 xCameraBoundaries;
        public Vector2 yCameraBoundaries;
        public Vector2 zCameraBoundaries;

        Meka meka;
        
        public override void Update(float deltaTime)
        {
            if(mekaTransform == null || mainCameraTransform == null) return;
            
            meka = mekaTransform.GetGameObject().GetComponent<Meka>();

            CenterCamera(deltaTime);
            CameraRotation();
            ControlMeka(deltaTime);


            bool floor = false;
            Vector3 position;
            Physics.RaycastHit hit;
            for (int i = 0; i < 4 && meka.speed.Y<=0 && !floor; i++)
            {
                position = new Vector3(i%2==0?-0.25f:0.25f,0, i >= 2 ? -0.25f : 0.25f);
                floor = Physics.Raycast(mekaTransform.TransformPosition(position), -Vector3.UnitY, 0.1f,out hit);
                if (floor)
                {
                    mekaTransform.position.Y -= hit.distance;
                    meka.speed.Y = 0;
                    meka.isFlying = false;
                }
            }

            if (!floor || meka.speed.Y>0)
            {
                meka.isFlying = true;
            }
        }

        private void CenterCamera(float deltaTime)
        {

            Vector3 position = mekaTransform.InverseTransformPosition(mainCameraTransform.position);
            float maxSpeed = meka.isFlying ? meka.maxFlightSpeed : meka.maxGroundSpeed;

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


            meka.InputMeka(deltaTime, input, rotation);

        }
    }
}
