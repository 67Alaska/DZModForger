using System;
using System.Diagnostics;

namespace DZModForger.Models
{
    /// <summary>
    /// Represents 3D transformation (position, rotation, scale)
    /// Used for object placement and manipulation in the viewport
    /// </summary>
    public class Transform3D
    {
        private Vector3D _position;
        private Vector3D _rotation;    // Euler angles in degrees
        private Vector3D _scale;
        private Matrix4D _transformMatrix;
        private bool _isDirty;

        public event EventHandler TransformChanged;

        public Transform3D()
        {
            Debug.WriteLine("[TRANSFORM3D] Creating new transform");

            _position = new Vector3D(0, 0, 0);
            _rotation = new Vector3D(0, 0, 0);
            _scale = new Vector3D(1, 1, 1);
            _isDirty = true;

            RecalculateMatrix();
        }

        // ==================== PROPERTIES ====================

        /// <summary>
        /// World position (X, Y, Z)
        /// </summary>
        public Vector3D Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// World position X coordinate
        /// </summary>
        public float PositionX
        {
            get => _position.X;
            set
            {
                if (Math.Abs(_position.X - value) > 0.0001f)
                {
                    _position.X = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// World position Y coordinate
        /// </summary>
        public float PositionY
        {
            get => _position.Y;
            set
            {
                if (Math.Abs(_position.Y - value) > 0.0001f)
                {
                    _position.Y = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// World position Z coordinate
        /// </summary>
        public float PositionZ
        {
            get => _position.Z;
            set
            {
                if (Math.Abs(_position.Z - value) > 0.0001f)
                {
                    _position.Z = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Rotation in Euler angles (degrees)
        /// </summary>
        public Vector3D Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Rotation around X axis (pitch) in degrees
        /// </summary>
        public float RotationX
        {
            get => _rotation.X;
            set
            {
                if (Math.Abs(_rotation.X - value) > 0.0001f)
                {
                    _rotation.X = value % 360.0f;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Rotation around Y axis (yaw) in degrees
        /// </summary>
        public float RotationY
        {
            get => _rotation.Y;
            set
            {
                if (Math.Abs(_rotation.Y - value) > 0.0001f)
                {
                    _rotation.Y = value % 360.0f;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Rotation around Z axis (roll) in degrees
        /// </summary>
        public float RotationZ
        {
            get => _rotation.Z;
            set
            {
                if (Math.Abs(_rotation.Z - value) > 0.0001f)
                {
                    _rotation.Z = value % 360.0f;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Non-uniform scale (X, Y, Z)
        /// </summary>
        public Vector3D Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Scale on X axis
        /// </summary>
        public float ScaleX
        {
            get => _scale.X;
            set
            {
                if (Math.Abs(_scale.X - value) > 0.0001f)
                {
                    _scale.X = Math.Max(0.001f, value);
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Scale on Y axis
        /// </summary>
        public float ScaleY
        {
            get => _scale.Y;
            set
            {
                if (Math.Abs(_scale.Y - value) > 0.0001f)
                {
                    _scale.Y = Math.Max(0.001f, value);
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Scale on Z axis
        /// </summary>
        public float ScaleZ
        {
            get => _scale.Z;
            set
            {
                if (Math.Abs(_scale.Z - value) > 0.0001f)
                {
                    _scale.Z = Math.Max(0.001f, value);
                    _isDirty = true;
                    OnTransformChanged();
                }
            }
        }

        /// <summary>
        /// Gets the combined transformation matrix
        /// </summary>
        public Matrix4D Matrix
        {
            get
            {
                if (_isDirty)
                {
                    RecalculateMatrix();
                }
                return _transformMatrix;
            }
        }

        // ==================== MATRIX OPERATIONS ====================

        /// <summary>
        /// Recalculates the transformation matrix from current TRS values
        /// </summary>
        private void RecalculateMatrix()
        {
            try
            {
                // Create scale matrix
                var scaleMatrix = Matrix4D.CreateScale(_scale.X, _scale.Y, _scale.Z);

                // Create rotation matrix (from Euler angles in order: Z, X, Y)
                var rotX = DegreesToRadians(_rotation.X);
                var rotY = DegreesToRadians(_rotation.Y);
                var rotZ = DegreesToRadians(_rotation.Z);

                var rotationMatrix = Matrix4D.CreateRotationZ(rotZ) *
                                    Matrix4D.CreateRotationX(rotX) *
                                    Matrix4D.CreateRotationY(rotY);

                // Create translation matrix
                var translationMatrix = Matrix4D.CreateTranslation(_position.X, _position.Y, _position.Z);

                // Combine: Translation * Rotation * Scale
                _transformMatrix = scaleMatrix * rotationMatrix * translationMatrix;

                _isDirty = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TRANSFORM3D] Exception in RecalculateMatrix: {ex.Message}");
            }
        }

        // ==================== TRANSFORMATIONS ====================

        /// <summary>
        /// Translates the object
        /// </summary>
        public void Translate(float x, float y, float z)
        {
            Position += new Vector3D(x, y, z);
        }

        /// <summary>
        /// Translates the object in a specific direction
        /// </summary>
        public void TranslateDirection(Vector3D direction, float distance)
        {
            Position += direction.Normalized() * distance;
        }

        /// <summary>
        /// Rotates the object around X axis (pitch)
        /// </summary>
        public void RotateX(float degrees)
        {
            RotationX += degrees;
        }

        /// <summary>
        /// Rotates the object around Y axis (yaw)
        /// </summary>
        public void RotateY(float degrees)
        {
            RotationY += degrees;
        }

        /// <summary>
        /// Rotates the object around Z axis (roll)
        /// </summary>
        public void RotateZ(float degrees)
        {
            RotationZ += degrees;
        }

        /// <summary>
        /// Rotates the object around arbitrary axis
        /// </summary>
        public void RotateAroundAxis(Vector3D axis, float degrees)
        {
            // Convert axis-angle to Euler angles (simplified)
            var radians = DegreesToRadians(degrees);
            var axisNorm = axis.Normalized();

            // Update rotation (simplified - proper implementation would use quaternions)
            RotationX += axisNorm.X * degrees;
            RotationY += axisNorm.Y * degrees;
            RotationZ += axisNorm.Z * degrees;
        }

        /// <summary>
        /// Scales the object uniformly
        /// </summary>
        public void ScaleUniform(float scale)
        {
            Scale = new Vector3D(scale, scale, scale);
        }

        /// <summary>
        /// Scales the object non-uniformly
        /// </summary>
        public void ScaleNonUniform(float x, float y, float z)
        {
            Scale = new Vector3D(
                Math.Max(0.001f, x),
                Math.Max(0.001f, y),
                Math.Max(0.001f, z)
            );
        }

        /// <summary>
        /// Resets transform to identity
        /// </summary>
        public void Reset()
        {
            Position = new Vector3D(0, 0, 0);
            Rotation = new Vector3D(0, 0, 0);
            Scale = new Vector3D(1, 1, 1);
        }

        // ==================== QUATERNION CONVERSION ====================

        /// <summary>
        /// Gets rotation as a quaternion
        /// </summary>
        public Quaternion GetQuaternion()
        {
            return Quaternion.FromEuler(_rotation.X, _rotation.Y, _rotation.Z);
        }

        /// <summary>
        /// Sets rotation from quaternion
        /// </summary>
        public void SetQuaternion(Quaternion q)
        {
            Rotation = q.ToEuler();
        }

        // ==================== UTILITY ====================

        private float DegreesToRadians(float degrees)
        {
            return degrees * (float)Math.PI / 180.0f;
        }

        private float RadiansToDegrees(float radians)
        {
            return radians * 180.0f / (float)Math.PI;
        }

        protected virtual void OnTransformChanged()
        {
            TransformChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Creates a deep copy of this transform
        /// </summary>
        public Transform3D Clone()
        {
            var clone = new Transform3D
            {
                Position = _position,
                Rotation = _rotation,
                Scale = _scale
            };
            return clone;
        }

        public override string ToString()
        {
            return $"Transform [Pos: {_position}, Rot: {_rotation}, Scale: {_scale}]";
        }
    }

    // ==================== VECTOR STRUCTURES ====================

    public struct Vector3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
            => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3D operator -(Vector3D a, Vector3D b)
            => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3D operator *(Vector3D a, float scalar)
            => new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public static bool operator ==(Vector3D a, Vector3D b)
            => Math.Abs(a.X - b.X) < 0.0001f && Math.Abs(a.Y - b.Y) < 0.0001f && Math.Abs(a.Z - b.Z) < 0.0001f;

        public static bool operator !=(Vector3D a, Vector3D b)
            => !(a == b);

        public float Dot(Vector3D other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public Vector3D Cross(Vector3D other)
            => new Vector3D(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

        public float Length()
            => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3D Normalized()
        {
            float len = Length();
            return len > 0.0001f ? new Vector3D(X / len, Y / len, Z / len) : this;
        }

        public override string ToString()
            => $"({X:F2}, {Y:F2}, {Z:F2})";

        public override bool Equals(object obj)
            => obj is Vector3D v && this == v;

        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z);
    }

    // ==================== MATRIX STRUCTURE ====================

    public struct Matrix4D
    {
        public float[,] M { get; private set; }

        public Matrix4D()
        {
            M = new float[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    M[i, j] = i == j ? 1.0f : 0.0f;
        }

        public static Matrix4D CreateTranslation(float x, float y, float z)
        {
            var m = new Matrix4D();
            m.M[0, 3] = x;
            m.M[1, 3] = y;
            m.M[2, 3] = z;
            return m;
        }

        public static Matrix4D CreateScale(float x, float y, float z)
        {
            var m = new Matrix4D();
            m.M[0, 0] = x;
            m.M[1, 1] = y;
            m.M[2, 2] = z;
            return m;
        }

        public static Matrix4D CreateRotationX(float radians)
        {
            var m = new Matrix4D();
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);
            m.M[1, 1] = c;
            m.M[1, 2] = -s;
            m.M[2, 1] = s;
            m.M[2, 2] = c;
            return m;
        }

        public static Matrix4D CreateRotationY(float radians)
        {
            var m = new Matrix4D();
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);
            m.M[0, 0] = c;
            m.M[0, 2] = s;
            m.M[2, 0] = -s;
            m.M[2, 2] = c;
            return m;
        }

        public static Matrix4D CreateRotationZ(float radians)
        {
            var m = new Matrix4D();
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);
            m.M[0, 0] = c;
            m.M[0, 1] = -s;
            m.M[1, 0] = s;
            m.M[1, 1] = c;
            return m;
        }

        public static Matrix4D operator *(Matrix4D a, Matrix4D b)
        {
            var result = new Matrix4D();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    for (int k = 0; k < 4; k++)
                        result.M[i, j] += a.M[i, k] * b.M[k, j];
            return result;
        }
    }

    // ==================== QUATERNION STRUCTURE ====================

    public struct Quaternion
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Quaternion FromEuler(float pitch, float yaw, float roll)
        {
            float p = pitch * (float)Math.PI / 360.0f;
            float y = yaw * (float)Math.PI / 360.0f;
            float r = roll * (float)Math.PI / 360.0f;

            float cp = (float)Math.Cos(p);
            float sp = (float)Math.Sin(p);
            float cy = (float)Math.Cos(y);
            float sy = (float)Math.Sin(y);
            float cr = (float)Math.Cos(r);
            float sr = (float)Math.Sin(r);

            return new Quaternion(
                sr * cp * cy - cr * sp * sy,
                cr * sp * cy + sr * cp * sy,
                cr * cp * sy - sr * sp * cy,
                cr * cp * cy + sr * sp * sy
            );
        }

        public Vector3D ToEuler()
        {
            float pitch = (float)Math.Atan2(2 * (W * X + Y * Z), 1 - 2 * (X * X + Y * Y));
            float yaw = (float)Math.Asin(2 * (W * Y - Z * X));
            float roll = (float)Math.Atan2(2 * (W * Z + X * Y), 1 - 2 * (Y * Y + Z * Z));

            return new Vector3D(
                pitch * 180.0f / (float)Math.PI,
                yaw * 180.0f / (float)Math.PI,
                roll * 180.0f / (float)Math.PI
            );
        }

        public override string ToString()
            => $"Quaternion ({X:F2}, {Y:F2}, {Z:F2}, {W:F2})";
    }
}
