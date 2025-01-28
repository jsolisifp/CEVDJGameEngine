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
        Rigidbody grabbed;
        Vector2 previousMousePosition;
        AudioSource audioSource;

        public override void Start()
        {
            grabbed = null;
            GameManager.state = GameManager.State.idle;
            GameManager.nextState = GameManager.State.idle;

            audioSource = gameObject.GetComponent<AudioSource>();

            previousMousePosition = Input.GetMousePosition();
        }

        public override void Update(float deltaTime)
        {
            if(deltaTime == 0) { return; }

            Vector2 mousePosition = Input.GetMousePosition();
            Vector2 deltaMousePoistion = mousePosition - previousMousePosition;

            if(!Input.IsMouseButtonPressed(1))
            { gameObject.transform.position += 0.001f * new Vector3(deltaMousePoistion.X, -deltaMousePoistion.Y, 0); }
            else
            { gameObject.transform.position += 0.001f * new Vector3(0, 0, deltaMousePoistion.Y); }
            
            previousMousePosition = mousePosition;

            Vector3 position = gameObject.transform.position;
            position.X = MathF.Max(MathF.Min(position.X, 0.6f), -0.6f);
            position.Y = MathF.Max(MathF.Min(position.Y, 0.7f), 0.2f);
            position.Z = MathF.Max(MathF.Min(position.Z, 0.5f), -0.5f);
            gameObject.transform.position = position;


            if (GameManager.state == GameManager.State.idle)
            {
                if (Input.IsMouseButtonPressed(0))
                {
                    GameManager.nextState = GameManager.State.grabbing;
                }
            }
            else if (GameManager.state == GameManager.State.grabbing)
            {
                if (grabbed != null)
                {
                    audioSource.Play();
                    grabbed.isKinematic = true;
                    Transform t = grabbed.GetGameObject().transform;
                    t.position = gameObject.transform.TransformPosition(grabbedOffset);
                    t.rotation = gameObject.transform.rotation;

                    float momentum = position.Z - position.Z * 20f;
                    grabbed.speed = new Vector3(0, 0, -momentum);

                    //grabbed.speed = new Vector3(0, 0, -10f);
                }

                // Tengo que cambiar de estado?
                if (!Input.IsMouseButtonPressed(0))
                {
                    if (grabbed != null)
                    {
                        grabbed.isKinematic = false;
                        grabbed = null;
                        GameManager.nextState = GameManager.State.rolling;
                    }
                    else
                    {
                        GameManager.nextState = GameManager.State.idle;
                    }                    
                }
            }
            else if (GameManager.state == GameManager.State.reseting)
            {
                gameObject.transform.position = new Vector3(0,0,0);
            }
        }

        public override void OnTriggerEnter(Rigidbody other)
        {
            grabbed = other;
        }
    }
}
