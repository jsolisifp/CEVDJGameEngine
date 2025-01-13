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
            grabbing,
            rolling,
            hitting
        }

        public static State state;
        public static State nextState;

        public AudioSource audioSource;

        public override void Start()
        {
            audioSource.Play();
        }

        public override void Update(float deltaTime)
        {
            // Cambios de estado

            if (GameManager.state != GameManager.nextState)
            {
                GameManager.state = GameManager.nextState;
            }
        }
    }
}
