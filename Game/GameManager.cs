using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class GameManager : Component
    {
        public enum State
        {
            idle,
            grabbing,
            going,
            impact
        }

        public static State state;
        public static State nextState;

        public override void Start()
        {
            state = State.idle;
        }

        public override void Update(float deltaTime)
        {
            Console.WriteLine(state);

            if (GameManager.state != GameManager.nextState)
            {
                GameManager.state = GameManager.nextState;
            }
        }
    }
}
