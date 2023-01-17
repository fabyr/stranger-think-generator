using System;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public static class Util
    {
        public const float Deg2Rad = 0.017453292f;
        public const float Rad2Deg = 57.29578049f;

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public static float VecAngleSigned(Vector3F a, Vector3F b, Vector3F n)
        {
            float angle = MathF.Acos(Vector3F.Dot(a.Normalized, b.Normalized));
            Vector3F cross = Vector3F.Cross(a, b);
            if (Vector3F.Dot(n, cross) < 0)
            { // Or > 0
                angle = -angle;
            }
            return angle;
        }

        public static float DistanceTo(this Vector3F a, Vector3F b)
        {
            return Vector3F.Distance(a, b);
        }

        public static float DistanceTo(this Vector2F a, Vector2F b)
        {
            return Vector2F.Distance(a, b);
        }

        public static Vector2F ProjectWeakPerspective(Vector3F p, float focalLength)
        {
            return new Vector2F()
            {
                X = (focalLength * p.X) / (focalLength + p.Z),
                Y = (focalLength * p.Y) / (focalLength + p.Z)
            };
        }

        public static void BresenhamLine(Vector2F a, Vector2F b, Action<int, int> pixelCallback)
        {
            int x, y, x2, y2;
            x = (int)a.X;
            y = (int)a.Y;
            x2 = (int)b.X;
            y2 = (int)b.Y;
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                pixelCallback(x, y);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public static Vector2F? LSegsIntersectionPoint(float p0_x, float p0_y, float p1_x, float p1_y, 
                                                       float p2_x, float p2_y, float p3_x, float p3_y)
        {
            float s1_x, s1_y, s2_x, s2_y;
            s1_x = p1_x - p0_x; s1_y = p1_y - p0_y;
            s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;

            float s, t;
            s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
            t = ( s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                // Collision detected
                return new Vector2F(p0_x + (t * s1_x), p0_y + (t * s1_y));
            }

            return null; // No collision
        }

        /*public static Vector2F? LSegsIntersectionPoint(Vector2F ps1, Vector2F pe1, Vector2F ps2, Vector2F pe2)
        {
            // Get A,B of first line - points : ps1 to pe1
            float A1 = pe1.Y - ps1.Y;
            float B1 = ps1.X - pe1.X;
            // Get A,B of second line - points : ps2 to pe2
            float A2 = pe2.Y - ps2.Y;
            float B2 = ps2.X - pe2.X;

            // Get delta and check if the lines are parallel
            float delta = A1 * B2 - A2 * B1;
            if (delta == 0) return null;

            // Get C of first and second lines
            float C2 = A2 * ps2.X + B2 * ps2.Y;
            float C1 = A1 * ps1.X + B1 * ps1.Y;
            //invert delta to make division cheaper
            float invdelta = 1 / delta;
            // now return the Vector2 intersection point
            return new Vector2F((B2 * C1 - B1 * C2) * invdelta, (A1 * C2 - A2 * C1) * invdelta);
        }*/

        public static Vector2F?[] RectIntersection(Vector2F p1, Vector2F p2, Vector2F r1, Vector2F r2, Vector2F r3, Vector2F r4, ref int n)
        {
            Tuple<Vector2F, Vector2F>[] arrangements = new Tuple<Vector2F, Vector2F>[]
            {
                Tuple.Create(r1, r2),
                Tuple.Create(r2, r3),
                Tuple.Create(r3, r4),
                Tuple.Create(r4, r1)
            };
            Vector2F?[] result = new Vector2F?[4];
            int c = 0;
            for (int i = 0; i < result.Length; i++)
            {
                Vector2F? intersection = LSegsIntersectionPoint(p1.X, p1.Y, p2.X, p2.Y, arrangements[i].Item1.X, arrangements[i].Item1.Y, arrangements[i].Item2.X, arrangements[i].Item2.Y);
                if (intersection != null)
                {
                    result[i] = intersection.Value;
                    c++;
                    if (c == n)
                        break;
                }
            }
            n = c;
            return result.ToArray();
        }
    }
}