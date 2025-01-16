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
            switch (GameManager.GetPinsDown())
            {
                case 0:
                    renderer.textureId = "0.png";
                    break;
                case 1:
                    renderer.textureId = "1.png";
                    break;
                case 2:
                    renderer.textureId = "2.png";
                    break;
                case 3:
                    renderer.textureId = "3.png";
                    break;
                case 4:
                    renderer.textureId = "4.png";
                    break;
                case 5:
                    renderer.textureId = "5.png";
                    break;
                case 6:
                    renderer.textureId = "6.png";
                    break;
                case 7:
                    renderer.textureId = "7.png";
                    break;
                case 8:
                    renderer.textureId = "8.png";
                    break;
                case 9:
                    renderer.textureId = "9.png";
                    break;
                case 10:
                    renderer.textureId = "10.png";
                    break;
            }
        }
    }
}
