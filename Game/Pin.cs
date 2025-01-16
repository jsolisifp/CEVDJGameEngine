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
        public Rigidbody rb;

        Vector3 originalPos;
        Vector3 originalRot;

        bool down;

        public override void Start()
        {
            down = false;
            originalPos = gameObject.transform.position;
            originalRot = gameObject.transform.rotation;
        }
        public override void Update(float deltaTime)
        {
            if (GameManager.state == GameManager.State.reseting)
            {
                if(GameManager.GetTries() > 0)
                {
                    if (gameObject.transform.rotation.X > 1 || gameObject.transform.rotation.X < -1 && !down)
                    {
                        Thrown();
                    }
                }
                else
                {
                    down = false;
                    rb.isKinematic = true;
                    gameObject.transform.rotation = originalRot;
                    gameObject.transform.position = originalPos;
                }
            }
            else
            {
                rb.isKinematic = false;
            }
        }

        public void Thrown()
        {
            down = true;
            GameManager.PinDown();
        }
    }
}
