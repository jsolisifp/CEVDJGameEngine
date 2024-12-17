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
        public Vector3 grabbedOffset = new Vector3(-0.05f, -0.1f, -0.1f);

        enum State
        {
            idle,
            grabbing
        };
        
        Vector2 mousePrevious;

        State state;
        State nextState;

        Rigidbody grabbed;

        public override void Start()
        {
            mousePrevious = Input.GetMousePosition();

            nextState = State.idle;
            nextState = State.idle;

        }

        public override void Update(float deltaTime)
        {

            if(deltaTime > 0)
            {
                Vector2 mouseDelta = Input.GetMousePosition() - mousePrevious;

                if(Input.IsMouseButtonPressed(1))
                { gameObject.transform.position += 0.01f * new Vector3(mouseDelta.X, -mouseDelta.Y, 0); }
                else { gameObject.transform.position += 0.01f * new Vector3(mouseDelta.X, 0, mouseDelta.Y); }

                mousePrevious = Input.GetMousePosition();
            }

            Vector3 p = gameObject.transform.position;
            p.X = MathF.Max(MathF.Min(p.X, 0.450f), -0.450f);
            p.Y = MathF.Max(MathF.Min(p.Y, 0.540f), 0.260f);
            p.Z = MathF.Max(MathF.Min(p.Z, 0.5f), -0.5f);
            gameObject.transform.position = p;

            if(state == State.grabbing)
            {
                Transform t = grabbed.GetGameObject().transform;
                t.position = gameObject.transform.TransformPosition(grabbedOffset);
                t.rotation = gameObject.transform.rotation;
                grabbed.speed = new Vector3(0, 0, -5f);

                if(!Input.IsMouseButtonPressed(0))
                {
                    nextState = State.idle;
                }
            }

            if(state != nextState)
            {
                if(nextState == State.grabbing)
                {
                    grabbed.isKinematic = true;
                }
                else
                {
                    grabbed.isKinematic = false;
                    grabbed = null;
                }

                state = nextState;
            }

            
        }

        public override void OnTriggerStay(Rigidbody other)
        {
            if(other.GetGameObject().name == "Ball" && Input.IsMouseButtonPressed(0))
            {
                grabbed = other;
                nextState = State.grabbing;
            }
        }
    }
}
