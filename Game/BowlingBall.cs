using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class BowlingBall : Component
    {
        public override void OnCollisionEnter(Physics.Collision collision)
        {
            if (collision.rigidbody.GetGameObject().name.Contains("BowlingPin"))
            {
                GameManager.nextState = GameManager.State.impact;
            }
        }
    }
}
