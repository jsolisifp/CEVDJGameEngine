using System.Numerics;

namespace GameEngine
{
    internal class Transform : Component
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public Transform()
        {
            position = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
            scale = new Vector3(1, 1, 1);
        }

        public override void Update(float deltaTime)
        {
        }

        public void LookAt(Vector3 position, Vector3 up)
        {
            Transform t = gameObject.transform;
            Matrix4x4 view = Matrix4x4.CreateLookAt(t.position, position, up);
            Matrix4x4 rotate;
            Matrix4x4.Invert(view, out rotate);
            Quaternion rotationQ = Quaternion.CreateFromRotationMatrix(rotate);
            Vector3 eulerRads = MathUtils.QuaternionToEuler(rotationQ);
            t.rotation = MathUtils.RadiansToDegrees(eulerRads);
        }

        public Vector3 TransformPosition(Vector3 position)
        {
            Matrix4x4 model = GetModelMatrix();
            Vector4 position4 = new Vector4(position, 1);
            Vector4 r = Vector4.Transform(position4, model);

            return new Vector3(r.X / r.W, r.Y / r.W, r.Z / r.W);

        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            Matrix4x4 model = GetModelMatrix();
            Vector4 direction4 = new Vector4(direction, 0);
            Vector4 r = Vector4.Transform(direction4, model);

            return new Vector3(r.X, r.Y, r.Z);

        }

        public Vector3 InverseTransformPosition(Vector3 position)
        {
            Matrix4x4 inverseModel = GetInverseModelMatrix();
            Vector4 position4 = new Vector4(position, 1);
            Vector4 r = Vector4.Transform(position4, inverseModel);

            return new Vector3(r.X / r.W, r.Y / r.W, r.Z / r.W);

        }

        Matrix4x4 GetModelMatrix()
        {
            float rotX = MathUtils.DegreesToRadians(rotation.X);
            float rotY = MathUtils.DegreesToRadians(rotation.Y);
            float rotZ = MathUtils.DegreesToRadians(rotation.Z);

            Matrix4x4 m = Matrix4x4.CreateScale(scale) *
                                Matrix4x4.CreateRotationX(rotX) *
                                Matrix4x4.CreateRotationY(rotY) *
                                Matrix4x4.CreateRotationZ(rotZ) *
                                Matrix4x4.CreateTranslation(position);

            return m;
        }

        Matrix4x4 GetInverseModelMatrix()
        {
            var result = Matrix4x4.Identity;
            Matrix4x4.Invert(GetModelMatrix(), out result);

            return result;
        }

        public Vector3 GetForward()
        {
            Vector3 v = new Vector3(0, 0, 1);

            return TransformDirection(v);
        }

        public Vector3 GetRight()
        {
            Vector3 v = new Vector3(1, 0, 0);

            return TransformDirection(v);
        }

        public Vector3 GetUp()
        {
            Vector3 v = new Vector3(0, 1, 0);

            return TransformDirection(v);
        }

    }
}
