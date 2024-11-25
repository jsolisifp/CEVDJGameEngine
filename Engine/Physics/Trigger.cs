using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;

namespace GameEngine
{
    internal class Trigger : Component
    {
        BoxCollider boxCollider;
        SphereCollider sphereCollider;

        BodyHandle handle;
        StaticHandle staticHandle;

        public override void Start()
        {
            boxCollider = gameObject.GetComponent<BoxCollider>();
            sphereCollider = gameObject.GetComponent<SphereCollider>();

            if (boxCollider == null && sphereCollider == null) { return; }

            Physics.ColliderType colliderType;

            TypedIndex colliderIndex;
            if (boxCollider != null) { colliderIndex = boxCollider.GetTypeIndex(); colliderType = Physics.ColliderType.box; }
            else { colliderIndex = sphereCollider.GetTypeIndex(); colliderType = Physics.ColliderType.sphere; }

            Physics.BodyOwner owner = new Physics.BodyOwner();
            owner.SetTrigger(this);

            if (gameObject.@static)
            {
                staticHandle = Physics.RegisterStaticBody(gameObject.transform.position, gameObject.transform.rotation, colliderIndex, owner);
            }
            else
            {
                handle = Physics.RegisterNonStaticBody(gameObject.transform.position,
                                              gameObject.transform.rotation, 1,
                                              Vector3.Zero, Vector3.Zero, true,
                                              colliderIndex, colliderType, owner);
            }

        }

        public override void FixedUpdate(float deltaTime)
        {
            if (!gameObject.@static)
            {
                Physics.SetKinematicBodyState(handle, gameObject.transform.position, gameObject.transform.rotation, Vector3.Zero, Vector3.Zero);
            }
        }

        public override void Stop()
        {
            if (boxCollider == null && sphereCollider == null) { return; }

            if (gameObject.@static)
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
