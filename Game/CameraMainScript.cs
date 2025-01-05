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
        public override void Update(float deltaTime)
        {
            if (GameManager.state == GameManager.State.grabbing && Input.IsMouseButtonPressed(1))
            {
                gameObject.transform.position = new Vector3(3.5f, 3, -2);
                gameObject.transform.rotation = new Vector3(-20, 90, 20);
            }
            if (GameManager.state == GameManager.State.rolling)
            {
                gameObject.transform.position = new Vector3(0, 1, -13);
                gameObject.transform.rotation = new Vector3(-20, 0, 0);
            }
            else
            {
                gameObject.transform.position = new Vector3(0, 1, 2);
                gameObject.transform.rotation = new Vector3(-20, 0, 0);
            }
        }
    }
}
