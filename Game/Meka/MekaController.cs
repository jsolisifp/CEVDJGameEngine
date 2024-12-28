using System;
using System.Collections.Generic;
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

        public override void Update(float deltaTime)
        {
            if(mekaTransform == null || mainCameraTransform == null) return;
            

            CenterCamera(deltaTime);
            CameraRotation();
            ControlMeka(deltaTime);
        }

        private void CenterCamera(float deltaTime)
        {
            Vector3 position = mekaTransform.TransformPosition(cameraOffset);

            if (mainCameraTransform.position.X != position.X)
            {
                if (mainCameraTransform.position.X < position.X)
                {
                    mainCameraTransform.position.X += centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.X > position.X) mainCameraTransform.position.X = position.X;
                }
                else
                {
                    mainCameraTransform.position.X -= centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.X < position.X) mainCameraTransform.position.X = position.X;
                }
            }

            if (mainCameraTransform.position.Y != position.Y)
            {
                if (mainCameraTransform.position.Y < position.Y)
                {
                    mainCameraTransform.position.Y += centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.Y > position.Y) mainCameraTransform.position.Y = position.Y;
                }
                else
                {
                    mainCameraTransform.position.Y -= centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.Y < position.Y) mainCameraTransform.position.Y = position.Y;
                }
            }

            if (mainCameraTransform.position.Z != position.Z)
            {
                if (mainCameraTransform.position.Z < position.Z)
                {
                    mainCameraTransform.position.Z += centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.Z > position.Z) mainCameraTransform.position.Z = position.Z;
                }
                else
                {
                    mainCameraTransform.position.Z -= centeringSpeed * deltaTime;
                    if (mainCameraTransform.position.Z < position.Z) mainCameraTransform.position.Z = position.Z;
                }
            }
        }

        private void CameraRotation()
        {
            Meka meka = mekaTransform.GetGameObject().GetComponent<Meka>();
            if (!meka.isAiming)
            {
                
            }
            else
            {
                mainCameraTransform.LookAt(meka.currentAim, Vector3.UnitY);
            }
            mainCameraTransform.rotation.X = 0;
            mainCameraTransform.rotation.Y = mekaTransform.rotation.Y + 180;
            mainCameraTransform.rotation.Z = 0;
        }

        Vector3 input;
        float rotation;
        private void ControlMeka(float deltaTime)
        {
            Meka meka = mekaTransform.GetGameObject().GetComponent<Meka>();

            if (Input.IsKeyPressed(Key.W)) { input.Z = 1; }
            else if (Input.IsKeyPressed(Key.S)) { input.Z = -1; }
            else { input.Z = 0; }

            if (Input.IsKeyPressed(Key.A)) { input.X = 1; }
            else if (Input.IsKeyPressed(Key.D)) { input.X = -1; }
            else { input.X = 0; }

            if (Input.IsKeyPressed(Key.Space)) { input.Y = 1; }
            else { input.Y = 0; }

            if (Input.IsKeyPressed(Key.Q)) { rotation = 1; }
            else if (Input.IsKeyPressed(Key.E)) { rotation = -1; }
            else { rotation = 0; }

            if(Input.IsKeyPressed(Key.F)) meka.isAiming = !meka.isAiming;

            meka.InputMeka(deltaTime, input, rotation);

        }
    }
}
