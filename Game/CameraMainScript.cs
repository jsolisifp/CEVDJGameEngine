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
        private Transform transform;

        public Transform ball;
        private Vector3 offset = new Vector3(0, 1, 1.5f);
        public float smoothTime = 0.1f;

        public override void Start()
        {
            transform = gameObject.transform;
        }
        public override void Update(float deltaTime)
        {
            if (GameManager.state == GameManager.State.grabbing && Input.IsMouseButtonPressed(1))
            {
                transform.position = new Vector3(0, 1.5f, 1f);
                transform.rotation = new Vector3(-40, 0, 0);
            }
            else if (GameManager.state == GameManager.State.rolling)
            {
                transform.rotation = new Vector3(-20, 0, 0);

                Vector3 ballPosition = ball.position;
                
                transform.position = Vector3.Lerp(transform.position, ballPosition + offset, smoothTime);
            }
            else if (GameManager.state == GameManager.State.hitting)
            {

            }
            else
            {
                transform.position = new Vector3(0, 1, 2);
                transform.rotation = new Vector3(-20, 0, 0);
            }
        }
    }
}
