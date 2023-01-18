using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Bezier
    {
        public Vector3F a, b, c, d;

        public Bezier(Vector3F a, Vector3F b, Vector3F c, Vector3F d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public override string ToString()
        {
            return $"Bezier[{a}, {b}, {c}, {d}]";
        }

        public Vector3F Point(float t)
        {
            Vector3F ab = Vector3F.Lerp(a, b, t);
            Vector3F bc = Vector3F.Lerp(b, c, t);
            Vector3F cd = Vector3F.Lerp(c, d, t);

            Vector3F abbc = Vector3F.Lerp(ab, bc, t);
            Vector3F bccd = Vector3F.Lerp(bc, cd, t);

            Vector3F abbbcbccd = Vector3F.Lerp(abbc, bccd, t);
            return abbbcbccd;
        }
    }
}