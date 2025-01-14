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

            if (GameManager.state == GameManager.State.reseting)
            {
                timer = 0;

                rb.isKinematic = true;
                gameObject.transform.position = new Vector3(0, 1, 0);
                gameObject.transform.rotation = new Vector3(0, 0, 0);
                rb.speed = new Vector3(0, 0, 0);
                rb.angularSpeed = new Vector3(0, 0, 0);
                GameManager.nextState = GameManager.State.idle;
            }
            else
            {
                rb.isKinematic = false;
            }

            if (GameManager.state == GameManager.State.hitting) 
            {
                timer += deltaTime;
                if (timer >= 5) 
                {
                    GameManager.nextState = GameManager.State.reseting;
                }
            }
        }
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            if (collision.rigidbody.GetGameObject().name.Contains("BowlingPin"))
            {
                GameManager.nextState = GameManager.State.hitting;
            }
        }
    }
}
