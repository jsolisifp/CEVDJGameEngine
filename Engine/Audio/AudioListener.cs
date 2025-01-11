using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class AudioListener : Component
    {
        public Transform transform;
        public float volume = 0.5f;

        public override void Update(float deltaTime)
        {
            transform = gameObject.transform;
        }
    }
}
