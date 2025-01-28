using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Pin : Component
    {
        private bool isFallen = false;
        public bool Isfallen { get { return isFallen; } }

        private Vector3 initialPosition;
        private Vector3 initialRotation;
        private Rigidbody rb;

        public override void Start()
        {
            isFallen = false;
            initialPosition = gameObject.transform.position;
            initialRotation = gameObject.transform.rotation;

            rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            
        }

        public override void Update(float deltaTime)
        {
            if (!isFallen && (Math.Abs(gameObject.transform.rotation.X) > 0.5f ||
                               Math.Abs(gameObject.transform.rotation.Z) > 0.5f))
            {
                isFallen = true;
                GameManager.Instance.AddScore(1);
            }
        }

        public override void OnCollisionEnter(Physics.Collision collision)
        {
            if (collision.OtherGameObject.name == "Ball" || collision.OtherGameObject.name.Contains("Pin"))
            {
                if (rb.isKinematic)
                {
                    rb.isKinematic = false;

                    Vector3 collisionForce = collision.rigidbody.speed * rb.mass * 0.8f;
                    rb.AddForce(collisionForce, Physics.ForceMode.impulse);

                    if (!isFallen)
                    {
                        isFallen = true;
                        GameManager.Instance.AddScore(1);
                    }
                }
            }
        }

        public bool IsStationary()
        {
            return rb.speed.Length() < 0.01f && rb.angularSpeed.Length() < 0.01f;
        }


        public void Reset()
        {
            gameObject.transform.position = initialPosition;
            gameObject.transform.rotation = initialRotation;
            rb.isKinematic = true;
            rb.speed = Vector3.Zero;
            rb.angularSpeed = Vector3.Zero;
            isFallen = false;
        }

    }
}
