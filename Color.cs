using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Color
    {
        public byte R, G, B;

        public Color(byte r, byte b, byte g)
        {
            R = r;
            B = b;
            G = g;
        }

        public override string ToString()
        {
            return $"Color[{R}, {G}, {B}]";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if(obj != null && obj is Color c)
                return c.R == R && c.G == G && c.B == B;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = 37;

                result *= 397;
                result += R.GetHashCode();

                result *= 397;
                result += G.GetHashCode();

                result *= 397;
                result += B.GetHashCode();

                return result;
            }
        }

        public static bool operator ==(Color? a, Color? b)
        {
            return a?.Equals(b) ?? (a is null && b is null);
        }

        public static bool operator !=(Color? a, Color? b)
        {
            return !(a == b);
        }
    }
}