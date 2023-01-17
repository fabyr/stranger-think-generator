using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Edge
    {
        public Vector3F A, B;

        public override string ToString()
        {
            return $"Edge[{A}, {B}]";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj != null && obj is Edge e)
                return e.A == A && e.B == B;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 37;

                result *= 397;
                result += A.GetHashCode();

                result *= 397;
                result += B.GetHashCode();

                return result;
            }
        }

        public static bool operator ==(Edge? a, Edge? b)
        {
            return a?.Equals(b) ?? (a is null && b is null);
        }

        public static bool operator !=(Edge? a, Edge? b)
        {
            return !(a == b);
        }
    }
}