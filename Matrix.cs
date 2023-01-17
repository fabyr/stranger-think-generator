using System;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace StrangerThinkGenerator
{
    public struct Matrix
    {
        public static Matrix XRot(float angle)
        {
            Matrix m = new Matrix(3, 3);
            m[0, 0] = 1;
            m[1, 1] = MathF.Cos(angle);
            m[2, 1] = -MathF.Sin(angle);
            m[1, 2] = MathF.Sin(angle);
            m[2, 2] = MathF.Cos(angle);
            return m;
        }

        public static Matrix YRot(float angle)
        {
            Matrix m = new Matrix(3, 3);
            m[0, 0] = MathF.Cos(angle);
            m[2, 0] = -MathF.Sin(angle);
            m[1, 1] = 1;
            m[0, 2] = MathF.Sin(angle);
            m[2, 2] = MathF.Cos(angle);
            return m;
        }

        public static Matrix ZRot(float angle)
        {
            Matrix m = new Matrix(3, 3);
            m[0, 0] = MathF.Cos(angle);
            m[1, 0] = -MathF.Sin(angle);
            m[0, 1] = MathF.Sin(angle);
            m[1, 1] = MathF.Cos(angle);
            m[2, 2] = 1;
            return m;
        }

        public float[,] mat;

        public int Width => mat.GetLength(0);
        public int Height => mat.GetLength(1);

        public Matrix(int w, int h)
        {
            mat = new float[w, h];
        }

        public float this[int x, int y]
        {
            get => mat[x, y];
            set => mat[x, y] = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Matrix[");
            for(int y = 0; y < Height; y++)
            {
                for(int x = 0; x < Width; x++)
                {
                    if(x != 0)
                        sb.Append(", ");
                    sb.Append(this[x, y]);
                }
                sb.AppendLine();
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if(a.Width != b.Height)
                throw new ArgumentException("The column-count of the first matrix must be equal to the row-count of the second matrix.");
            Matrix result = new Matrix(b.Width, a.Height);
            for(int x = 0; x < a.Height; x++)
            {
                for(int y = 0; y < b.Width; y++)
                {
                    float sum = 0;
                    for(int k = 0; k < a.Width; k++)
                        sum += a[k, x] * b[y, k];
                    result[y, x] = sum;
                }
            }
            return result;
        }
    }
}