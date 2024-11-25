using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GameEngine
{
    internal class FollowCamera : Component
    {
        public Transform target;

        public override void Update(float deltaTime)
        {
            gameObject.transform.position = target.position + new Vector3(0, 3, -10);
            gameObject.transform.LookAt(target.position + new Vector3(0, 2, 0), Vector3.UnitY);
        }
    }
}
