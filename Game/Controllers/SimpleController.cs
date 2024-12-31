using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class SimpleController : Component
    {
        public Vector3 input;
        public float rotation;
        public Transform mekaTransform;
        Meka meka;

        public override void Start()
        {
            meka = mekaTransform.GetGameObject().GetComponent<Meka>();
        }

        public override void Update(float deltaTime)
        {
            meka.InputMeka(input, rotation, null);
        }
    }
}
