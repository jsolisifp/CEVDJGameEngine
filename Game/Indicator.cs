using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class Indicator : Component
    {
        public Renderer renderer;
        public override void Update(float deltaTime)
        {
            if(gameObject.name == "Light1")
            {
                if (GameManager.triesLeft < 2) 
                {
                    renderer.textureId = "Red.png";
                }
            }
            else
            {
                if (GameManager.triesLeft < 1)
                {
                    renderer.textureId = "Red.png";
                }
            }
        }
    }
}
