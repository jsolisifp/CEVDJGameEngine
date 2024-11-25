using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;

namespace GameEngine
{
    internal class Rigidbody : Component
    {
        public Vector3 speed = new Vector3(0, 0, 0);
        public Vector3 angularSpeed = new Vector3(0, 0, 0);

        public float mass = 1.0f;
        public bool isKinematic = false;

        BoxCollider boxCollider;
        SphereCollider sphereCollider;

        BodyHandle handle;
        StaticHandle staticHandle;

        Vector3 accumulatedForce;
        Vector3 accumulatedAcceleration;

        Vector3 accumulatedTorque;
        Vector3 accumulatedAngularAcceleration;


        public override void Start()
        {
            boxCollider = gameObject.GetComponent<BoxCollider>();
            sphereCollider = gameObject.GetComponent<SphereCollider>();

            if(boxCollider == null && sphereCollider == null) { return; }

            Physics.ColliderType colliderType; 

            TypedIndex colliderIndex;
            if(boxCollider != null) { colliderIndex = boxCollider.GetTypeIndex(); colliderType = Physics.ColliderType.box; }
            else { colliderIndex = sphereCollider.GetTypeIndex(); colliderType = Physics.ColliderType.sphere; }

            Physics.BodyOwner owner = new Physics.BodyOwner();
            owner.SetRigidbody(this);

            if (gameObject.@static)
            {
                staticHandle = Physics.RegisterStaticBody(gameObject.transform.position, gameObject.transform.rotation, colliderIndex, owner);
            }
            else
            {
                handle = Physics.RegisterNonStaticBody(gameObject.transform.position,
                                              gameObject.transform.rotation, mass,
                                              speed, angularSpeed, isKinematic,
                                              colliderIndex, colliderType, owner);
            }

            accumulatedForce = Vector3.Zero;
            accumulatedAcceleration = Vector3.Zero;
            accumulatedTorque = Vector3.Zero;
            accumulatedAngularAcceleration = Vector3.Zero;

        }

        public override void FixedUpdate(float deltaTime)
        {
            if(!gameObject.@static)
            {
                if(isKinematic)
                {
                    Physics.SetKinematicBodyState(handle, gameObject.transform.position, gameObject.transform.rotation, speed, angularSpeed);
                }
                else
                {
                    Physics.AddLinearImpulse(accumulatedForce * deltaTime, handle);
                    Physics.AddVelocity(accumulatedAcceleration * deltaTime, handle);
                    Physics.AddAngularImpulse(accumulatedTorque * deltaTime, handle);
                    Physics.AddAngularVelocity(accumulatedAngularAcceleration * deltaTime, handle);
                    Physics.GetNonStaticBodyState(handle, out gameObject.transform.position, out gameObject.transform.rotation, out speed, out angularSpeed);

                    accumulatedForce = Vector3.Zero;
                    accumulatedAcceleration = Vector3.Zero;
                    accumulatedTorque = Vector3.Zero;
                    accumulatedAngularAcceleration = Vector3.Zero;

                }
            }
        }

        public void AddForce(Vector3 amount, Physics.ForceMode mode)
        {
            if(!gameObject.@static && !isKinematic)
            {                
                if(mode == Physics.ForceMode.force)
                {
                    accumulatedForce += amount;
                }
                else if(mode == Physics.ForceMode.impulse)
                {
                    Physics.AddLinearImpulse(amount, handle);
                }
                else if(mode == Physics.ForceMode.velocityChange)
                {
                    Physics.AddVelocity(amount, handle);
                }
                else // mode == Physics.ForceMode.acceleration
                {
                    accumulatedAcceleration += amount;
                }
            }
        }

        //public void AddForceAtPosition(Vector3 amount, Vector3 position, Physics.ForceMode mode)
        //{
        //    if (!gameObject.@static && !isKinematic)
        //    {
        //        if(mode == Physics.ForceMode.force)
        //        {
        //        }
        //        else if(mode == Physics.ForceMode.impulse)
        //        {
        //            Physics.AddLinearImpulseAt(amount, position, handle);
        //        }
        //        else if(mode == Physics.ForceMode.)
        //        {
        //        }
        //    }
        //}

        public void AddTorque(Vector3 amount, Physics.ForceMode mode)
        {
            if (!gameObject.@static && !isKinematic)
            {
                if (mode == Physics.ForceMode.force)
                {
                    accumulatedTorque += amount;
                }
                else if (mode == Physics.ForceMode.impulse)
                {
                    Physics.AddAngularImpulse(amount, handle);
                }
                else if (mode == Physics.ForceMode.velocityChange)
                {
                    Physics.AddAngularVelocity(amount, handle);
                }
                else // mode == Physics.ForceMode.acceleration
                {
                    accumulatedAngularAcceleration += amount;
                }
            }
        }

        public override void Stop()
        {
            if (boxCollider == null && sphereCollider == null) { return; }

            if(gameObject.@static)
            {
                Physics.UnregisterStaticBody(staticHandle);
            }
            else
            {
                Physics.UnregisterNonStaticBody(handle);
            }

        }


    }
}
