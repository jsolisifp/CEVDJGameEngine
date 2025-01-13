using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class Ball : Component
    {
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            GameManager.nextState = GameManager.State.hitting;
        }
    }
}
