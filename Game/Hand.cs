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
            Vector2 mousePosition = Input.GetMousePosition();

            Vector2 deltaMousePosition = mousePosition - previousMousePosition;

            if(Input.IsMouseButtonPressed(1))
            {
                gameObject.transform.position += deltaTime * new Vector3(deltaMousePosition.X, deltaMousePosition.Y, 0);
            }
            else
            {
                gameObject.transform.position += deltaTime * new Vector3(deltaMousePosition.X, 0, deltaMousePosition.Y);
            }

            Vector3 position = gameObject.transform.position;
            if (position.X < -2f) { position.X = -2f; }
            else if(position.X > 2f) { position.X = 2f; }

            if (position.Y < -2f) { position.Y = -2f; }
            else if(position.Y > 2f) { position.Y = 2f; }

            if (position.Z < -2f) { position.Z = -2f; }
            else if(position.Z > 2f) { position.Z = 2f; }



            previousMousePosition = mousePosition;

            if(state == State.idle)
            {
                //Comportamiento normal cuando no tengo objeto
            }
            else // state == grabing
            {
                //comportamiento normal tengo objeto
                Transform t = grabbed.GetGameObject().transform;
                t.position = gameObject.transform.position;
                t.rotation = gameObject.transform.rotation;

                if (!Input.IsMouseButtonPressed(0))
                {
                    nextState = State.idle;
                }
            }

            // Cambios de estado

            if(state != nextState)
            {
                if(nextState == State.grabbing) {
                    // comportamiento al entrar en grabbing
                    grabbed.isKinematic = true;
                }
                else
                {
                    // comportamiento al entrar en idle
                    grabbed.isKinematic= false;
                    grabbed = null;
                }
                state = nextState;
            }
        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            if(state == State.idle && Input.IsMouseButtonPressed(0) && other.GetGameObject().name == "Ball")
            {
                grabbed = other;
                nextState = State.grabbing;
            }
        }
    }
}
