using BepuPhysics.Collidables;
using System.Numerics;

namespace GameEngine
{
    internal class BoxCollider : Component
    {
        public Vector3 size = new Vector3(1, 1, 1);

        TypedIndex index;

        bool started;

        public override void Start()
        {
            if(started) { return; }

            index = Physics.RegisterBoxCollider(Vector3.Multiply(size, gameObject.transform.scale));

            started = true;
        }

        public override void Stop()
        {
            Physics.UnregisterCollider(index);

            started = false;
        }

        public TypedIndex GetTypeIndex()
        {
            if(!started) { Start(); }

            return index;
        }

    }
}
