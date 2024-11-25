﻿using System.Numerics;

namespace GameEngine
{
    public static class MathUtils
    {
        public static float DegreesToRadians(float degrees)
        {
            return MathF.PI / 180f * degrees;
        }

        public static Vector3 DegreesToRadians(Vector3 v)
        {
            return new Vector3(DegreesToRadians(v.X),
                               DegreesToRadians(v.Y),
                               DegreesToRadians(v.Z));
        }

        public static float RadiansToDegrees(float rads)
        {
            return 180f / MathF.PI * rads;
        }

        public static Vector3 RadiansToDegrees(Vector3 v)
        {
            return new Vector3(RadiansToDegrees(v.X),
                               RadiansToDegrees(v.Y),
                               RadiansToDegrees(v.Z));
        }

        //
        // https://stackoverflow.com/questions/70462758/c-sharp-how-to-convert-quaternions-to-euler-angles-xyz
        //
        public static Quaternion EulerToQuaternion(float rotX, float rotY, float rotZ)
        {
            float cy = (float)Math.Cos(rotX * 0.5f);
            float sy = (float)Math.Sin(rotX * 0.5f);
            float cp = (float)Math.Cos(rotY * 0.5f);
            float sp = (float)Math.Sin(rotY * 0.5f);
            float cr = (float)Math.Cos(rotZ * 0.5f);
            float sr = (float)Math.Sin(rotZ * 0.5f);

            Quaternion q = new Quaternion();
            q.W = cr * cp * cy + sr * sp * sy;
            q.X = sr * cp * cy - cr * sp * sy;
            q.Y = cr * sp * cy + sr * cp * sy;
            q.Z = cr * cp * sy - sr * sp * cy;
            return q;
        }


        public static Quaternion EulerToQuaternion(Vector3 rot)
        {
            return EulerToQuaternion(rot.X, rot.Y, rot.Z);
        }

        //
        // https://stackoverflow.com/questions/70462758/c-sharp-how-to-convert-quaternions-to-euler-angles-xyz
        //
        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            Vector3 angles = new Vector3();

            // rotZ (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

    }
}