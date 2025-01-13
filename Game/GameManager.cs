using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Game
{
    internal class GameManager : Component
    {
        public Vector3 pin01Position = new Vector3( 00.000f, 22.860f, 00.190f);
        public Vector3 pin02Position = new Vector3(-00.153f, 23.124f, 00.190f);
        public Vector3 pin03Position = new Vector3( 00.153f, 23.124f, 00.190f);
        public Vector3 pin04Position = new Vector3(-00.305f, 23.387f, 00.190f);
        public Vector3 pin05Position = new Vector3( 00.000f, 23.387f, 00.190f);
        public Vector3 pin06Position = new Vector3( 00.305f, 23.387f, 00.190f);
        public Vector3 pin07Position = new Vector3(-00.458f, 23.650f, 00.190f);
        public Vector3 pin08Position = new Vector3(-00.153f, 23.650f, 00.190f);
        public Vector3 pin09Position = new Vector3( 00.153f, 23.650f, 00.190f);
        public Vector3 pin10Position = new Vector3( 00.458f, 23.650f, 00.190f);

        public Rigidbody pin01;
        public Rigidbody pin02;
        public Rigidbody pin03;
        public Rigidbody pin04;
        public Rigidbody pin05;
        public Rigidbody pin06;
        public Rigidbody pin07;
        public Rigidbody pin08;
        public Rigidbody pin09;
        public Rigidbody pin10;

        public Transform welcome;
        public Transform success;
        public Transform fail;

        enum State
        {
            welcome,
            showPinsLeft,
            grabBall,
            moveBall,
            ballThrowed,
            success,
            fail
        }

        State state;
        State nextState;

        Rigidbody[] pins;
        Vector3[] pinPositions;
        bool[] pinsDown;

        public override void Start()
        {
            pins = new Rigidbody[10];
            pins[0] = pin01;
            pins[1] = pin02;
            pins[2] = pin03;
            pins[3] = pin04;
            pins[4] = pin05;
            pins[5] = pin06;
            pins[6] = pin07;
            pins[7] = pin08;
            pins[8] = pin09;
            pins[9] = pin10;

            pinPositions = new Vector3[10];

            pinPositions[0] = pin01Position;
            pinPositions[1] = pin02Position;
            pinPositions[2] = pin03Position;
            pinPositions[3] = pin04Position;
            pinPositions[4] = pin05Position;
            pinPositions[5] = pin06Position;
            pinPositions[6] = pin07Position;
            pinPositions[7] = pin08Position;
            pinPositions[8] = pin09Position;
            pinPositions[9] = pin10Position;

            state = State.welcome;
            nextState = State.welcome;

            ResetPins(true);

            state = State.welcome;
            nextState = State.welcome;
        }

        public override void Update(float deltaTime)
        {

            if(state == State.welcome)
            {
                Console.WriteLine("Welcome");
            }

        }

        public override void FixedUpdate(float deltaTime)
        {
        }

        void ResetPins(bool resetDown = false)
        {
            for (int i = 0; i < pins.Length; i++)
            {
                if(resetDown)
                {
                    pinsDown[i] = false;
                }

                if(!pinsDown[i])
                {
                    Transform t = pins[i].GetGameObject().transform;
                    t.position = pinPositions[i];
                    t.rotation = Vector3.Zero;
                }
            }

        }

    }
}
