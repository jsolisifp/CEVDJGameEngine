using System.Numerics;

namespace GameEngine
{
    internal class Camera : Component
    {
        public Vector3 backgroundColor = new Vector3(0, 0, 1);
        public float fov = 60.0f;
        public float zNear = 0.1f;
        public float zFar = 1000.0f;

        public override void Update(float deltaTime)
        {
            Transform t = gameObject.transform;
            GameEngine.Render.SetClearColor(backgroundColor);
            GameEngine.Render.SetView(t.position, t.rotation, fov, zNear, zFar);
        }
    }
}
