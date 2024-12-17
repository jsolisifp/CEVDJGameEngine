using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            if(deltaTime == 0) { return; }

            Vector2 mousePosition = Input.GetMousePosition();
            Vector2 deltaMousePoistion = mousePosition - previousMousePosition;

            if(Input.IsMouseButtonPressed(1))
            { gameObject.transform.position += 0.01f * new Vector3(deltaMousePoistion.X, -deltaMousePoistion.Y, 0); }
            else
            { gameObject.transform.position += 0.01f * new Vector3(deltaMousePoistion.X, 0, deltaMousePoistion.Y); }
            
            previousMousePosition = mousePosition;

            Vector3 position = gameObject.transform.position;
            if(position.X < -2.0f) { position.X = -2.0f; }
            else if (position.X > 2.0f) { position.X = 2.0f; };

            if (position.Y < 0) { position.Y = -0.0f; }
            else if (position.Y > 1.0f) { position.Y = 2.0f; };

            if (position.Z < -2.0f) { position.Z = -2.0f; }
            else if (position.Z > 2.0f) { position.Z = 2.0f; };

            gameObject.transform.position = position;

            if (state == State.idle)
            {
                // Comportamiento normal cuando no tengo objeto
            }
            else // state == State.grabbing
            {
                Transform t = grabbed.GetGameObject().transform;
                t.position = gameObject.transform.position;
                t.rotation = gameObject.transform.rotation;

                // Tengo que cambiar de estado?

                if(!Input.IsMouseButtonPressed(0))
                {
                    nextState = State.idle;
                }
            }

            // Cambios de estado

            if (state != nextState)
            {
                if (nextState == State.grabbing)
                {
                    grabbed.isKinematic = true;
                }
                else // nextState == State.idle
                {
                    grabbed.isKinematic = false;
                    grabbed = null;
                }

                state = nextState;
            }

        }

        public override void OnTriggerEnter(Rigidbody other)
        {

            if (state == State.idle && Input.IsMouseButtonPressed(0) && other.GetGameObject().name == "BowlingBall")
            {
                grabbed = other;
                nextState = State.grabbing;
            }

        }

    }
}
