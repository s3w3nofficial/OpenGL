using OpenTK;
using System;

namespace OpenGL
{
    public class Camera
    {
        public const float NearPlane = 0.1f;
        public const float FarPlane = 1000f;
        public const float Fov = 70;

        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }

        private float _pitch;
        public float _yaw = MathHelper.PiOver2;

        public Vector3 pos;
        
        public Vector2 left
        {
            get
            {
                var s = (float)Math.Sin(-(_yaw + MathHelper.PiOver2));
                var c = (float)Math.Cos(_yaw + MathHelper.PiOver2);

                return new Vector2(s, c).Normalized();
            }
        }

        public Vector2 forward
        {
            get
            {
                var s = -(float)Math.Sin(-_yaw);
                var c = -(float)Math.Cos(_yaw);

                return new Vector2(s, c).Normalized();
            }
        }

        public Camera()
        {
            UpdateViewMatrix();
            UpdateProjectionMatrix();
        }

        public float pitch
        {
            get => _pitch;

            set => _pitch = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        public void UpdateViewMatrix()
        {
            var x = Matrix4.CreateRotationX(pitch);
            var y = Matrix4.CreateRotationY(_yaw);

            var t = Matrix4.CreateTranslation(-pos);

            View = t * y * x;
        }

        public void UpdateProjectionMatrix()
        {
            var matrix = Matrix4.Identity;

            var aspectRatio = (float) Game.Instance.Width / Game.Instance.Height;
            var yScale = (float) (1f / Math.Tan(MathHelper.DegreesToRadians(Fov / 2f)));
            var xScale = yScale / aspectRatio;
            var frustumLength = FarPlane - NearPlane;

            matrix.M11 = xScale;
            matrix.M22 = yScale;
            matrix.M33 = -((FarPlane + NearPlane) / frustumLength);
            matrix.M34 = -1;
            matrix.M43 = -(2 * NearPlane * FarPlane / frustumLength);
            matrix.M44 = 0;

            Projection = matrix;
        }
        
    }
}