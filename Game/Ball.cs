using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class Ball : Component
    {
        public Rigidbody rb;
        float timer = 0;
        public override void Update(float deltaTime)
        {

            if (GameManager.state == GameManager.State.idle)
            {
                if (GameManager.GetTries() == 0 && GameManager.GetPinsDown() != 10)
                {
                    GameManager.nextState = GameManager.State.reseting;
                }
            }
            else if (GameManager.state == GameManager.State.rolling)
            {
                timer += deltaTime;
                if (timer > 5 && gameObject.transform.position.Z > -1) 
                {
                    GameManager.nextState = GameManager.State.reseting;
                }
            }
            else if (GameManager.state == GameManager.State.hitting)
            {
                timer += deltaTime;
                if (GameManager.GetTries() > 0)
                {
                    if (timer >= 3)
                    {
                        GameManager.nextState = GameManager.State.reseting;
                    }
                }
                else
                {
                    if (timer >= 10)
                    {
                        GameManager.nextState = GameManager.State.reseting;
                    }
                }
            }
            else if (GameManager.state == GameManager.State.reseting)
            {
                timer = 0;

                rb.isKinematic = true;
                gameObject.transform.position = new Vector3(0, 0.5f, 0);
                gameObject.transform.rotation = new Vector3(0, 0, 0);
                rb.speed = new Vector3(0, 0, 0);
                rb.angularSpeed = new Vector3(0, 0, 0);

                GameManager.nextState = GameManager.State.idle;
            }

            if (!(GameManager.state == GameManager.State.reseting))
            {
                rb.isKinematic = false;
            }
        }
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            if (collision.rigidbody.GetGameObject().name.Contains("BowlingPin"))
            {
                timer = 0;
                GameManager.nextState = GameManager.State.hitting;
            }
        }
    }
}
