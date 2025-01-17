using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class Counter : Component
    {
        public Renderer renderer;
        public override void Update(float deltaTime)
        {
            renderer.textureId = GameManager.GetPinsDown() + ".png";
        }
    }
}
