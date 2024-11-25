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

            if(Input.IsKeyPressed(Key.Space))
            {
                Physics.RaycastHit hit;
                if (Physics.Raycast(t.position + t.GetForward() * 3 + Vector3.UnitY * 0.2f, t.GetForward(), 5, out hit))
                {
                    Rigidbody rigid = hit.transform.GetGameObject().GetComponent<Rigidbody>();
                
                    if(rigid != null)
                    {
                        rigid.AddForce(gameObject.transform.GetForward() * 100, Physics.ForceMode.impulse);
                    }
                    Console.WriteLine("Hitted " + hit.transform.GetGameObject().name);

                }
            }

        }

        public override void OnCollisionEnter(Physics.Collision collision)
        {
            //Console.WriteLine("Entering collision with " + collision.transform.GetGameObject().name);

            for(int i = 0; i < collision.contactsCount; i++)
            {
                Physics.Contact c = collision.contactList[collision.contactsOffset + i];
                //Console.WriteLine("Point " + "(" + c.position.X + ", " + c.position.Y + ", " + c.position.Z);
            }
        }

        public override void OnCollisionStay(Physics.Collision collision)
        {
            //Console.WriteLine("Staying in collision with " + collision.transform.GetGameObject().name);

            for (int i = 0; i < collision.contactsCount; i++)
            {
                Physics.Contact c = collision.contactList[collision.contactsOffset + i];
                //Console.WriteLine("Point " + "(" + c.position.X + ", " + c.position.Y + ", " + c.position.Z);
            }

            collision.rigidbody.AddForce(Vector3.UnitY * 1000, Physics.ForceMode.force);
            //collision.rigidbody.AddForce(Vector3.UnitY * 100, Physics.ForceMode.acceleration);

            //collision.rigidbody.AddTorque(Vector3.UnitY * 1000, Physics.ForceMode.force);
            collision.rigidbody.AddTorque(Vector3.UnitY * 100, Physics.ForceMode.force);

        }

        public override void OnCollisionExit(Physics.Collision collision)
        {
            //Console.WriteLine("Exiting collision with " + collision.transform.GetGameObject().name);

            for (int i = 0; i < collision.contactsCount; i++)
            {
                Physics.Contact c = collision.contactList[collision.contactsOffset + i];
                //Console.WriteLine("Point " + "(" + c.position.X + ", " + c.position.Y + ", " + c.position.Z);
            }
        }
    }
}
