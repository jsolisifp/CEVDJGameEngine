using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine
{
    internal class Hand : Component
    {
        Rigidbody grabbed;
        Vector2 previousMousePosition;

        public override void Start()
        {
            grabbed = null;

            GameManager.state = GameManager.State.idle;
            GameManager.nextState = GameManager.State.idle;

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

            if (position.X < -1.5f)
            {
                position.X = -1.5f;
            }
            else if (position.X > 1.5f)
            {
                position.X = 1.5f;
            }

            if (position.Y < 0f)
            {
                position.Y = 0.0f;
            }
            else if (position.Y > 1.0f)
            {
                position.Y = 1.0f;
            }

            if (position.Z < -1f)
            {
                position.Z = -1f;
            }
            else if (position.Z > 1f)
            {
                position.Z = 1f;
            }

            gameObject.transform.position = position;


            if (GameManager.state == GameManager.State.idle)
            {
                if (Input.IsMouseButtonPressed(0))
                {
                    GameManager.nextState = GameManager.State.grabbing;
                }
            }

            else // state == State.grabbing
            {
                Transform t = grabbed.GetGameObject().transform;

                t.position = gameObject.transform.position;
                t.rotation = gameObject.transform.rotation;

                if (!Input.IsMouseButtonPressed(0))
                {
                    if (grabbed != null)
                    {
                        //grabbed.isKinematic = false;
                        grabbed.AddForce(new Vector3(0, 0, -70f), Physics.ForceMode.impulse);
                        grabbed = null;
                        GameManager.nextState = GameManager.State.going;
                    }
                    else
                    {
                        GameManager.nextState = GameManager.State.idle;
                    }
                }
            }

            //Cambios Estado

            if (GameManager.state != GameManager.nextState)
            {
                if (GameManager.nextState == GameManager.State.grabbing)
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

                GameManager.state = GameManager.nextState;
            }

        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            if (GameManager.state == GameManager.State.idle && Input.IsMouseButtonPressed(0) && other.GetGameObject().name == "BowlingBowl")
            {
                grabbed = other;
                GameManager.nextState = GameManager.State.grabbing;
            }
        }

    }
}
