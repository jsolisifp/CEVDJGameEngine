using BepuPhysics.Collidables;

namespace GameEngine
{
    internal class SphereCollider : Component
    {
        public float radius = 1.0f;

        TypedIndex index;

        bool started;

        public override void Start()
        {
            if(started) { return; }

            index = Physics.RegisterSphereCollider(radius * gameObject.transform.scale.X);

            started = true;
        }

        public override void Stop()
        {
            Physics.UnregisterCollider(index);

            started = false;
        }

        public TypedIndex GetTypeIndex()
        {
            if (!started) { Start(); }

            return index;
        }

    }
}
