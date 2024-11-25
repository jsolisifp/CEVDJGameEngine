using System.Numerics;

namespace GameEngine
{
    internal class DirectionalLight : Component
    {
        public Vector3 color = new Vector3(1.0f, 1.0f, 1.0f);
        public float intensity = 1.0f;

        public override void Update(float deltaTime)
        {
            Vector3 direction = gameObject.transform.TransformDirection(Vector3.UnitZ);
            GameEngine.Render.SetDirectionalLight(direction, intensity, color);
        }

    }
}
