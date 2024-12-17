using Silk.NET.GLFW;
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
        State nextstate;

        Vector2 previousMousePosition;
        public override void Start()
        {
            grabbed = null;
            state = State.idle;
            nextstate = State.idle;

            previousMousePosition = Input.GetMousePosition();
        }

        public override void Update(float deltaTime)
        {
            Vector2 mouseposition = Input.GetMousePosition();
            Vector2 deltaMousePosition = mouseposition - previousMousePosition;

            if (Input.IsMouseButtonPressed(1))
            {
                gameObject.transform.position += 0.02f * new Vector3(deltaMousePosition.X,deltaMousePosition.Y, 0);
            }
            else
            {
                gameObject.transform.position += 0.02f * new Vector3(deltaMousePosition.X, 0, deltaMousePosition.Y);
            }
            previousMousePosition = mouseposition;

            Vector3 position = gameObject.transform.position;

            if (position.X < -3.0f)
            {
                position.X = -2.0f;
            }else if(position.X > 2.0f)
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
            else if (position.Z > 2.5f)
            {
                position.Z = 2.5f;
            }

            gameObject.transform.position = position;



            if (state == State.idle) 
            {
                //comportamiento normal cuando no tengo objeto


            }
            else // state == State.grabbing
            {
                //Comportamiento normal tengo objeto
                Transform t = grabbed.GetGameObject().transform;
                t.position = gameObject.transform.position;
                t.rotation = gameObject.transform.rotation;

                //Tengo que cambiar de estado?
                if (!Input.IsMouseButtonPressed(0))
                {
                    nextstate = State.idle;
                }
            }

            //Cambios de estado
            if (state != nextstate)
            {
                if (nextstate == State.grabbing)
                {
                    //Comportamiento al entrar en grabbing
                    grabbed.isKinematic = true;

                }
                else //nextState == State.idle
                {
                    //Comportamiento al entrar en idle

                }


                state = nextstate;
            }
        }


        public override void OnTriggerEnter(Rigidbody other)
        {
            if (state == State.idle && Input.IsMouseButtonPressed(0) && other.GetGameObject().name == "BowlingBall")
            {
                grabbed = other;
                nextstate= State.grabbing;
            }


        }


    }
}
