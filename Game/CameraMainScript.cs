using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class CameraMainScript : Component
    {
        public Transform ball;
        public override void Update(float deltaTime)
        {
            if (GameManager.state == GameManager.State.grabbing && Input.IsMouseButtonPressed(1))
            {
                gameObject.transform.position = new Vector3(3.5f, 3, -2);
                gameObject.transform.rotation = new Vector3(-20, 90, 20);
            }
            else if (GameManager.state == GameManager.State.rolling)
            {
                gameObject.transform.position += ball.position + new Vector3(0, 1, 1.5f) - gameObject.transform.position;
                gameObject.transform.rotation = new Vector3(-20, 0, 0);
            }
            else if (GameManager.state == GameManager.State.hitting)
            {

            }
            else
            {
                gameObject.transform.position = new Vector3(0, 1, 2);
                gameObject.transform.rotation = new Vector3(-20, 0, 0);
            }
        }
    }
}
