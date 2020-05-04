using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace DromeEd
{
    public abstract class Transformable
    {
        // From page 4 & 5 of https://www.vectornav.com/docs/default-source/documentation/vn-100-documentation/AN002.pdf
        public static Vector3 QuatToEuler(Quaternion quat)
        {
            float pitch = (float)Math.Asin(-2.0f * (quat.X * quat.Z - quat.Y * quat.W));
            float yaw = (float)Math.Atan2(2.0f * (quat.X * quat.Y + quat.Z * quat.W),
                quat.W * quat.W - quat.Z * quat.Z - quat.Y * quat.Y + quat.X * quat.X);
            float roll = (float)Math.Atan2(2.0f * (quat.Y * quat.Z + quat.X * quat.W),
                quat.W * quat.W + quat.Z * quat.Z - quat.Y * quat.Y - quat.X * quat.X);
            return new Vector3(pitch, yaw, roll);
        }

        public abstract Matrix Transform { get; set; }

        public Vector3 Translation {
            get {
                return Transform.TranslationVector;
            }
            set {
                Matrix transform = Transform;
                transform.TranslationVector = value;
                Transform = transform;
            }
        }

        public Vector3 Scale
        {
            get
            {
                return new Vector3(
                    (float)Math.Sqrt((Transform.M11 * Transform.M11) + (Transform.M12 * Transform.M12) + (Transform.M13 * Transform.M13)),
                    (float)Math.Sqrt((Transform.M21 * Transform.M21) + (Transform.M22 * Transform.M22) + (Transform.M23 * Transform.M23)),
                    (float)Math.Sqrt((Transform.M31 * Transform.M31) + (Transform.M32 * Transform.M32) + (Transform.M33 * Transform.M33)));
            }
            set
            {
                Vector3 xNorm = new Vector3(Transform.M11, Transform.M12, Transform.M13);
                Vector3 yNorm = new Vector3(Transform.M21, Transform.M22, Transform.M23);
                Vector3 zNorm = new Vector3(Transform.M31, Transform.M32, Transform.M33);
                xNorm.Normalize();
                yNorm.Normalize();
                zNorm.Normalize();
                Matrix transform = Transform;
                transform.M11 = xNorm.X * value.X;
                transform.M12 = xNorm.Y * value.X;
                transform.M13 = xNorm.Z * value.X;
                transform.M21 = yNorm.X * value.Y;
                transform.M22 = yNorm.Y * value.Y;
                transform.M23 = yNorm.Z * value.Y;
                transform.M31 = zNorm.X * value.Z;
                transform.M32 = zNorm.Y * value.Z;
                transform.M33 = zNorm.Z * value.Z;
                Transform = transform;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                Transform.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                Vector3 euler = QuatToEuler(rotation);
                return new Vector3(MathUtil.RadiansToDegrees(euler.X), MathUtil.RadiansToDegrees(euler.Y), MathUtil.RadiansToDegrees(euler.Z));
            }
            set
            {
                Vector3 translation = Translation;
                Vector3 scale = Scale;
                Transform = Matrix.Transformation(Vector3.Zero, Quaternion.Identity, Scale, Vector3.Zero, Quaternion.RotationYawPitchRoll(MathUtil.DegreesToRadians(value.X), MathUtil.DegreesToRadians(value.Y), MathUtil.DegreesToRadians(value.Z)), Translation);
            }
        }
    }
}
