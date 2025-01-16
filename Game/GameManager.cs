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
        public static int triesLeft;
        public int previousTry;
        public static int pinsDown;
        public enum State
        {
            idle,
            grabbing,
            rolling,
            hitting,
            reseting
        }

        public static State state;
        public static State nextState;

        public AudioSource audioSource;

        public override void Start()
        {
            audioSource.Play();
            triesLeft = 2;
            previousTry = 2;
            pinsDown = 0;
            state = State.idle;
        }

        public override void Update(float deltaTime)
        {
            Console.WriteLine(state);
            Console.WriteLine("Tries: " + triesLeft);
            Console.WriteLine("Pins Down: " + pinsDown);
            // Cambios de estado

            if (state != nextState)
            {
                if (state == State.reseting) 
                {
                    previousTry = triesLeft;
                }
                if(nextState == State.reseting)
                {
                    switch (previousTry)
                    {
                        case 2:
                            triesLeft = 1;
                            break;
                        case 1:
                            triesLeft = 0;
                            break;
                        case 0:
                            pinsDown = 0;
                            triesLeft = 2;
                            break;
                    }

                }
                state = nextState;
            }
        }

        public static int GetTries()
        {
            return triesLeft;
        }
        public static int GetPinsDown()
        {
            return pinsDown;
        }
        public static void PinDown()
        {
            pinsDown++;
        }
    }
}
