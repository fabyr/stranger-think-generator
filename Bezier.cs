using System;

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

        public override readonly string ToString()
        {
            return $"Bezier[{a}, {b}, {c}, {d}]";
        }

        public readonly Vector3F Point(float t)
        {
            if (t < 0)
                t = 0;
            if (t > 1)
                t = 1;
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