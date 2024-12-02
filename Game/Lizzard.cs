using Silk.NET.Input;
using System.Numerics;

namespace GameEngine
{
    internal class Lizzard : Component
    {
        public float speed = 5.0f;
        public float rotationSpeed = 120.0f;

        public override void Update(float deltaTime)
        {
            Transform t = gameObject.transform;

            float r = t.rotation.Y * MathF.PI / 180;
            Vector3 forward = new Vector3(MathF.Sin(r), 0, MathF.Cos(r));

            float multiplier = Input.IsKeyPressed(Key.ShiftLeft) || Input.IsKeyPressed(Key.ShiftRight) ? 4 : 1;

            if (Input.IsKeyPressed(Key.W))
            {
                t.position += forward * speed * multiplier * deltaTime;
            }
            else if (Input.IsKeyPressed(Key.S))
            {
                t.position -= forward * speed * multiplier * deltaTime;
            }

            if (Input.IsKeyPressed(Key.A))
            {
                t.rotation.Y += rotationSpeed * multiplier * deltaTime;
            }
            else if (Input.IsKeyPressed(Key.D))
            {
                t.rotation.Y -= rotationSpeed * multiplier * deltaTime;
            }


        }
    }
}
