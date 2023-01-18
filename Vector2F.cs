using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Vector2F
    {
        public static readonly Vector2F Zero = new Vector2F();

        public float X, Y;

        public float Magnitude => MathF.Sqrt(X * X + Y * Y);
        public Vector2F Normalized
        {
            get
            {
                float m = Magnitude;
                return new Vector2F(X / m, Y / m);
            }
        }

        public Vector2F(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"Vector2[{X}, {Y}]";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj != null && obj is Vector2F p)
                return p.X == X && p.Y == Y;
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

                return result;
            }
        }

        public static bool operator ==(Vector2F? a, Vector2F? b)
        {
            return a?.Equals(b) ?? (a is null && b is null);
        }

        public static bool operator !=(Vector2F? a, Vector2F? b)
        {
            return !(a == b);
        }

        public static Vector2F operator *(Vector2F a, float b)
        {
            return new Vector2F()
            {
                X = a.X * b,
                Y = a.Y * b
            };
        }

        public static Vector2F operator *(Vector2F a, Vector3F b)
        {
            return new Vector2F()
            {
                X = a.X * b.X,
                Y = a.Y * b.Y
            };
        }

        public static Vector2F operator +(Vector2F a, Vector2F b)
        {
            return new Vector2F()
            {
                X = a.X + b.X,
                Y = a.Y + b.Y
            };
        }

        public static Vector2F operator -(Vector2F a, Vector2F b)
        {
            return new Vector2F()
            {
                X = a.X - b.X,
                Y = a.Y - b.Y
            };
        }

        public static float Distance(Vector2F a, Vector2F b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}