using System;
using System.Text;

namespace StrangerThinkGenerator
{
    public struct Matrix
    {
        public static Matrix XRot(float angle)
        {
            Matrix m = new(3, 3);
            m[0, 0] = 1;
            m[1, 1] = MathF.Cos(angle);
            m[2, 1] = -MathF.Sin(angle);
            m[1, 2] = MathF.Sin(angle);
            m[2, 2] = MathF.Cos(angle);
            return m;
        }

        public static Matrix YRot(float angle)
        {
            Matrix m = new(3, 3);
            m[0, 0] = MathF.Cos(angle);
            m[2, 0] = -MathF.Sin(angle);
            m[1, 1] = 1;
            m[0, 2] = MathF.Sin(angle);
            m[2, 2] = MathF.Cos(angle);
            return m;
        }

        public static Matrix ZRot(float angle)
        {
            Matrix m = new(3, 3);
            m[0, 0] = MathF.Cos(angle);
            m[1, 0] = -MathF.Sin(angle);
            m[0, 1] = MathF.Sin(angle);
            m[1, 1] = MathF.Cos(angle);
            m[2, 2] = 1;
            return m;
        }

        public float[,] mat;

        public readonly int Width => mat.GetLength(0);
        public readonly int Height => mat.GetLength(1);

        public Matrix(int w, int h)
        {
            mat = new float[w, h];
        }

        public readonly float this[int x, int y]
        {
            get => mat[x, y];
            set => mat[x, y] = value;
        }

        public override readonly string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("Matrix[");
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x != 0)
                        sb.Append(", ");
                    sb.Append(this[x, y]);
                }
                sb.AppendLine();
            }
            sb.Append(']');
            return sb.ToString();
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Width != b.Height)
                throw new ArgumentException("The column-count of the first matrix must be equal to the row-count of the second matrix.");
            Matrix result = new(b.Width, a.Height);
            for (int x = 0; x < a.Height; x++)
            {
                for (int y = 0; y < b.Width; y++)
                {
                    float sum = 0;
                    for (int k = 0; k < a.Width; k++)
                        sum += a[k, x] * b[y, k];
                    result[y, x] = sum;
                }
            }
            return result;
        }

        // https://stackoverflow.com/questions/1148309/inverting-a-4x4-matrix
        // https://stackoverflow.com/a/23806710

        private static float InvF(int i, int j, Matrix m)
        {

            int o = 2 + (j - i);

            i += 4 + o;
            j += 4 - o;

            float e(int a, int b) => m[(i + a) % 4, (j + b) % 4];

            float inv =
            +e(+1, -1) * e(+0, +0) * e(-1, +1)
            + e(+1, +1) * e(+0, -1) * e(-1, +0)
            + e(-1, -1) * e(+1, +0) * e(+0, +1)
            - e(-1, -1) * e(+0, +0) * e(+1, +1)
            - e(-1, +1) * e(+0, -1) * e(+1, +0)
            - e(+1, -1) * e(-1, +0) * e(+0, +1);

            return (o % 2 != 0) ? inv : -inv;
        }

        public readonly Matrix? Inverse4x4()
        {
            if (Width != 4 || Height != 4)
                throw new InvalidOperationException("This method can only be called on a 4x4 matrix.");
            Matrix inv = new(4, 4);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    inv[i, j] = InvF(i, j, this);

            float D = 0;

            for (int k = 0; k < 4; k++) D += this[k, 0] * inv[0, k];

            if (D == 0) return null;

            D = 1.0f / D;

            for (int j = 0; j < 4; j++)
                for (int i = 0; i < 4; i++)
                    inv[i, j] = inv[i, j] * D;

            return inv;

        }

        public static Matrix RotationMatrix4x4(Vector3F eulerAngles)
        {
            float Sx = MathF.Sin(eulerAngles.X * Util.Deg2Rad);
            float Sy = MathF.Sin(eulerAngles.Y * Util.Deg2Rad);
            float Sz = MathF.Sin(eulerAngles.Z * Util.Deg2Rad);
            float Cx = MathF.Cos(eulerAngles.X * Util.Deg2Rad);
            float Cy = MathF.Cos(eulerAngles.Y * Util.Deg2Rad);
            float Cz = MathF.Cos(eulerAngles.Z * Util.Deg2Rad);
            Matrix m = new(4, 4);

            m[0, 0] = Cy * Cz;
            m[1, 0] = -Cy * Sz;
            m[2, 0] = Sy;
            m[0, 1] = Cz * Sx * Sy + Cx * Sz;
            m[1, 1] = Cx * Cz - Sx * Sy * Sz;
            m[2, 1] = -Cy * Sx;
            m[0, 2] = -Cx * Cz * Sy + Sx * Sz;
            m[1, 2] = Cz * Sx + Cx * Sy * Sz;
            m[2, 2] = Cx * Cy;

            m[3, 3] = 1;
            return m;
        }
    }
}