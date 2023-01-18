using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Vector3F
    {
        public static readonly Vector3F Zero = new Vector3F();
        public float X, Y, Z;

        public float Magnitude => MathF.Sqrt(X * X + Y * Y + Z * Z);
        public Vector3F Normalized
        {
            get
            {
                float m = Magnitude;
                return new Vector3F(X / m, Y / m, Z / m);
            }
        }

        public Vector3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"Vector3[{X}, {Y}, {Z}]";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj != null && obj is Vector3F p)
                return p.X == X && p.Y == Y && p.Z == Z;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 37;

                result *= 397;
                result += X.GetHashCode();

                result *= 397;
                result += Y.GetHashCode();

                result *= 397;
                result += Z.GetHashCode();

                return result;
            }
        }

        public static bool operator ==(Vector3F? a, Vector3F? b)
        {
            return a?.Equals(b) ?? (a is null && b is null);
        }

        public static bool operator !=(Vector3F? a, Vector3F? b)
        {
            return !(a == b);
        }

        public static Vector3F operator *(Vector3F a, float b)
        {
            return new Vector3F()
            {
                X = a.X * b,
                Y = a.Y * b,
                Z = a.Z * b
            };
        }

        public static Vector3F operator *(Vector3F a, Vector3F b)
        {
            return new Vector3F()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y,
                Z = a.Z * b.Z
            };
        }

        public static Vector3F operator +(Vector3F a, Vector3F b)
        {
            return new Vector3F()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y,
                Z = a.Z + b.Z
            };
        }

        public static Vector3F operator -(Vector3F a, Vector3F b)
        {
            return new Vector3F()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y,
                Z = a.Z - b.Z
            };
        }

        public static float Dot(Vector3F a, Vector3F b)
        {
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }

        public static Vector3F Cross(Vector3F a, Vector3F b)
        {
            Vector3F res = new Vector3F();
            res.X = a.Y * b.Z - a.Z * b.Y;
            res.Y = a.Z * b.X - a.X * b.Z;
            res.Z = a.X * b.Y - a.Y * b.X;
            return res;
        }

        public static float Distance(Vector3F a, Vector3F b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float dz = b.Z - a.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static Vector3F Lerp(Vector3F a, Vector3F b, float t)
        {
            return new Vector3F()
            {
                X = Util.Lerp(a.X, b.X, t),
                Y = Util.Lerp(a.Y, b.Y, t),
                Z = Util.Lerp(a.Z, b.Z, t)
            };
        }
    }
}