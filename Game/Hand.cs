using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Hand : Component
    {
        enum State
        {
            idle,
            grabbing
        }

        Rigidbody grabbed;

        State state;
        State nextState;

        Vector2 previousMousePosition;

        public override void Start()
        {
            grabbed = null;
            state = State.idle;
            nextState = State.idle;

            previousMousePosition = Input.GetMousePosition();
        }

        public override void Update(float deltaTime)
        {
            if (deltaTime == 0) { return; }

            Vector2 mousePosition = Input.GetMousePosition();
            Vector2 deltaMousePosition = mousePosition - previousMousePosition;

            if (Input.IsMouseButtonPressed(1))
            {
                gameObject.transform.position += 0.02f * new Vector3(deltaMousePosition.X, deltaMousePosition.Y, 0);
            }
            else
            {
                gameObject.transform.position += 0.02f * new Vector3(deltaMousePosition.X, 0, deltaMousePosition.Y);   
            }
            previousMousePosition = mousePosition;

            Vector3 position = gameObject.transform.position;

            if (position.X < -2.0f)
            {
                position.X = -2.0f;
            }
            else if (position.X > 2.0f)
            {
                position.X = 2.0f;
            }

            if (position.Y < 0f)
            {
                position.Y = 0.0f;
            }
            else if (position.Y > 1.0f)
            {
                position.Y = 1.0f;
            }

            if (position.Z < -0.5f)
            {
                position.Z = -0.5f;
            }
            else if (position.Z > 5f)
            {
                position.Z = 5f;
            }

            gameObject.transform.position = position;


            if (state == State.idle)
            {

            }
            else // state == State.grabbing
            {
                Transform t = grabbed.GetGameObject().transform;

                t.position = gameObject.transform.position;
                t.rotation = gameObject.transform.rotation;

                if (!Input.IsMouseButtonPressed(0))
                {
                    nextState = State.idle;
                }
            }

            //Cambios Estado

            if (state!= nextState)
            {
                if (nextState == State.grabbing)
                {
                    // Codigo al entrar en grabbing
                    grabbed.isKinematic = true;

                }
                else // nextState == state.idle
                {
                    //Codgio al entrar en idle
                    grabbed.isKinematic = false;
                    grabbed = null;
                }

                state = nextState;
            }

        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            if (state == State.idle && Input.IsMouseButtonPressed(0) && other.GetGameObject().name == "BowlingBowl")
            {
                grabbed = other;
                nextState = State.grabbing;
            }
        }

    }
}
