using Silk.NET.GLFW;
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
            grabbing
        }

        public static State state;
        public static State nextState;
    }
}
