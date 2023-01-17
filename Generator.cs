using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace StrangerThinkGenerator
{
    public class Generator
    {
        public string Text { get; set; } = "Stranger\nThink";

        public int PointCount { get; set; } = 1000;

        public float ClosestNeighborCount { get; set; } = 5;
        public float ClosestNeighborConnectionProbability { get; set; } = 0.25f;

        public Color Background { get; set; } = new Color(0, 0, 0);
        public Color LineColor { get; set; } = new Color(255, 0, 0);

        public Vector3F BoundingBoxMin { get; set; } = new Vector3F(-50, -6, -0.8f);
        public Vector3F BoundingBoxMax { get; set; } = new Vector3F(12, 2, 3);
        
        private Vector3F[]? _vertices;
        private List<Edge> _edgeTable = new List<Edge>();
        private Random? _rnd;

        public Vector3F CameraPos { get; set; } = new Vector3F(-4, 0, 0.25f);
        private Vector3F _camRotCurrent;
        public Vector3F CameraRotation { get; set; } = new Vector3F(0, 0, 0);
        private Vector3F Forward => new Vector3F()
        {
            X = MathF.Cos(_camRotCurrent.X)*MathF.Cos(_camRotCurrent.Y),
            Y = MathF.Sin(_camRotCurrent.X)*MathF.Cos(_camRotCurrent.Y),
            Z = MathF.Sin(_camRotCurrent.Y)
        };
        public float FieldOfView { get; set; } = 100f;
        public float NearClipPlane { get; set; } = 0.01f;
        public float FarClipPlane { get; set; } = 200f;

        private Matrix _cam2world_translate, _cam2world_rot, _cam_clip_mat, _screen_mat;
        private float _verticalFieldOfView;

        public Generator()
        { }

        public void Setup(int seed = 133769420)
        {
            _edgeTable.Clear();
            _vertices = new Vector3F[8];
            _vertices[0] = new Vector3F(-1, -1, -1);
            _vertices[1] = new Vector3F(1, -1, -1);
            _vertices[2] = new Vector3F(1, 1, -1);
            _vertices[3] = new Vector3F(-1, 1, -1);

            _vertices[4] = new Vector3F(-1, -1, 1);
            _vertices[5] = new Vector3F(1, -1, 1);
            _vertices[6] = new Vector3F(1, 1, 1);
            _vertices[7] = new Vector3F(-1, 1, 1);

            _edgeTable.Add(new Edge() { A = _vertices[0], B = _vertices[1] });
            _edgeTable.Add(new Edge() { A = _vertices[1], B = _vertices[2] });
            _edgeTable.Add(new Edge() { A = _vertices[2], B = _vertices[3] });
            _edgeTable.Add(new Edge() { A = _vertices[3], B = _vertices[0] });

            _edgeTable.Add(new Edge() { A = _vertices[4], B = _vertices[5] });
            _edgeTable.Add(new Edge() { A = _vertices[5], B = _vertices[6] });
            _edgeTable.Add(new Edge() { A = _vertices[6], B = _vertices[7] });
            _edgeTable.Add(new Edge() { A = _vertices[7], B = _vertices[4] });

            _edgeTable.Add(new Edge() { A = _vertices[0], B = _vertices[4] });
            _edgeTable.Add(new Edge() { A = _vertices[1], B = _vertices[5] });
            _edgeTable.Add(new Edge() { A = _vertices[2], B = _vertices[6] });
            _edgeTable.Add(new Edge() { A = _vertices[3], B = _vertices[7] });

            /*_edgeTable.Clear();
            _rnd = new Random(seed);
            Func<float, float, float> gen = (min, max) => Util.Map(_rnd.NextSingle(), 0, 1, min, max);
            _vertices = new Vector3F[PointCount];
            for(int i = 0; i < PointCount; i++)
            {
                _vertices[i].X = gen(BoundingBoxMin.X, BoundingBoxMax.X);
                _vertices[i].Y = gen(BoundingBoxMin.Y, BoundingBoxMax.Y);
                _vertices[i].Z = gen(BoundingBoxMin.Z, BoundingBoxMax.Z);
            }

            bool[] visited = new bool[PointCount];
            List<Tuple<Vector3F, int>> buffer = new List<Tuple<Vector3F, int>>();
            Tuple<Vector3F, int>? pointOfInterest;
            for(int i = 0; i < PointCount; i++)
            {
                if(visited[i]) continue;
                pointOfInterest = Tuple.Create(_vertices[i], i);
                visited[i] = true;
                var min = _vertices.Select((x, i) => Tuple.Create(x, i))
                         .Where((x) => !visited[x.Item2])
                         .MinBy(x => x.Item1.DistanceTo(pointOfInterest.Item1));
                if(min == null)
                    break;
                buffer.Add(min);
                if(buffer.Count == ClosestNeighborCount)
                {
                    foreach(var tuple in buffer)
                    {
                        if(_rnd.NextSingle() < ClosestNeighborConnectionProbability)
                        {
                            _edgeTable.Add(new Edge() { A = pointOfInterest.Item1, B = tuple.Item1 });
                        }
                    }
                    buffer.Clear();
                }
            }*/
        }

        public void SetMatrices(float time, int imageWidth, int imageHeight)
        {
            _verticalFieldOfView = FieldOfView * (imageHeight / (float)imageWidth);

            _camRotCurrent = CameraRotation;
            //camRot.X = MathF.Sin(time * MathF.PI) * 180f;
            _camRotCurrent.Y += MathF.Sin(time * MathF.PI) * 180f;
            //camRot.Z = MathF.Sin(time * MathF.PI * 3.33f) * 180f;
            
            _cam2world_translate = new Matrix(4, 4);
            _cam2world_translate[0, 0] = 1;
            _cam2world_translate[1, 1] = 1;
            _cam2world_translate[3, 1] = 1;
            _cam2world_translate[2, 2] = 1;
            _cam2world_translate[3, 3] = 1;
            _cam2world_translate[0, 3] = -CameraPos.X;
            _cam2world_translate[1, 3] = -CameraPos.Y;
            _cam2world_translate[2, 3] = -CameraPos.Z;

            /*
            | cos(yaw)cos(pitch) -cos(yaw)sin(pitch)sin(roll)-sin(yaw)cos(roll) -cos(yaw)sin(pitch)cos(roll)+sin(yaw)sin(roll)|
            | sin(yaw)cos(pitch) -sin(yaw)sin(pitch)sin(roll)+cos(yaw)cos(roll) -sin(yaw)sin(pitch)cos(roll)-cos(yaw)sin(roll)|
            | sin(pitch)          cos(pitch)sin(roll)                            cos(pitch)sin(roll)|

            */

            //float pitch = camRot.X, yaw = camRot.Y, roll = camRot.Z;
            float Sx    = MathF.Sin(_camRotCurrent.X * Util.Deg2Rad);
            float Sy    = MathF.Sin(_camRotCurrent.Y * Util.Deg2Rad);
            float Sz    = MathF.Sin(_camRotCurrent.Z * Util.Deg2Rad);
            float Cx    = MathF.Cos(_camRotCurrent.X * Util.Deg2Rad);
            float Cy    = MathF.Cos(_camRotCurrent.Y * Util.Deg2Rad);
            float Cz    = MathF.Cos(_camRotCurrent.Z * Util.Deg2Rad);
            _cam2world_rot = new Matrix(4, 4);

            _cam2world_rot[0, 0] = Cy*Cz;
            _cam2world_rot[1, 0] = -Cy*Sz;
            _cam2world_rot[2, 0] = Sy;
            _cam2world_rot[0, 1] = Cz*Sx*Sy+Cx*Sz;
            _cam2world_rot[1, 1] = Cx*Cz-Sx*Sy*Sz;
            _cam2world_rot[2, 1] = -Cy*Sx;
            _cam2world_rot[0, 2] = -Cx*Cz*Sy+Sx*Sz;
            _cam2world_rot[1, 2] = Cz*Sx+Cx*Sy*Sz;
            _cam2world_rot[2, 2] = Cx*Cy;
            
            _cam2world_rot[3, 3] = 1;

            _cam_clip_mat = new Matrix(4, 4);
            float right = MathF.Tan(Util.Deg2Rad * FieldOfView * 0.5f), left = -right, top = MathF.Tan(Util.Deg2Rad * _verticalFieldOfView * 0.5f), bottom = -top;
            _cam_clip_mat[0, 0] = 2f / (right - left);
            _cam_clip_mat[1, 1] = 2f / (top - bottom);
            _cam_clip_mat[2, 2] = (FarClipPlane + NearClipPlane) / (FarClipPlane - NearClipPlane);
            _cam_clip_mat[2, 3] = -2 * NearClipPlane * FarClipPlane / (FarClipPlane - NearClipPlane);
            _cam_clip_mat[3, 2] = 1;

            //_cam_mat = Matrix.Multiply(_cam2world_translate, _cam2world_rot);

            _screen_mat = new Matrix(4, 4);
            _screen_mat[0, 0] = imageWidth / 2;
            _screen_mat[1, 1] = -imageHeight / 2;
            _screen_mat[2, 2] = 1;
            _screen_mat[3, 3] = 1;
            _screen_mat[0, 3] = imageWidth / 2;
            _screen_mat[1, 3] = imageHeight / 2;
        }

        public void RenderFrame(Image<Rgba32> image)
        {
            Rgba32 lineCol = new Rgba32(LineColor.R, LineColor.G, LineColor.B);
            image.Mutate(x => x.Fill(new Rgba32(Background.R, Background.G, Background.B)));
            Vector3F one = new Vector3F(0, 0, 1);
            foreach(Edge e in _edgeTable)
            {
                /*float d1 = Vector3F.Dot(Forward, (e.A - CameraPos));
                float d2 = Vector3F.Dot(Forward, (e.B - CameraPos));
                Console.WriteLine($"{Forward} - {CameraPos} - {e.A} - {d1}");
                if(d1 > 0 && d2 > 0) continue;*/

                Vector2F projectedA, projectedB;
                Vector3F pA3d, pB3d;
                
                bool va = Project(e.A, out projectedA, out pA3d);
                bool vb = Project(e.B, out projectedB, out pB3d);

                float d1 = Vector3F.Dot(one, pA3d);
                float d2 = Vector3F.Dot(one, pB3d);
                Console.WriteLine($"{CameraPos} - {pA3d} - {d1}");
                if(d1 < 0 && d2 < 0) continue;

                /*float d1 = Vector3F.Dot(e.A, e.B);
                float d = Vector3F.Dot(pA3d, pB3d);
                float angle1 = MathF.Acos(d1 / (e.A.Magnitude * e.B.Magnitude));
                float angle = MathF.Acos(d / (pA3d.Magnitude * pB3d.Magnitude));
                Console.WriteLine($"{d1} {angle1*Util.Rad2Deg} ; {d} {angle*Util.Rad2Deg}");*/
                
                /*float d1 = Vector3F.Dot(one, pA3d);
                float d2 = Vector3F.Dot(one, pB3d);
                d1 = MathF.Acos(d1 / (one.Magnitude * pA3d.Magnitude));
                d2 = MathF.Acos(d2 / (one.Magnitude * pB3d.Magnitude));
                Console.WriteLine($"{d1 * Util.Rad2Deg} {d2 * Util.Rad2Deg}");
                //if(MathF.Sign(d1) != MathF.Sign(d2)) continue;
                va &= d1 < MathF.PI * 0.5f;
                vb &= d2 < MathF.PI * 0.5f;*/
                /*Console.WriteLine($"{pA3d.Z} - {pB3d.Z}");
                if(pA3d.Z < 0 && pB3d.Z < 0) continue;*/

                if(!va || !vb) continue;
                //projectedA *= Scale;
                //projectedB *= Scale;
                //projectedA.Y *= -1;
                //projectedB.Y *= -1;
                //projectedA.X += image.Width * 0.5f;
                //projectedA.Y += image.Height * 0.5f;
                //projectedB.X += image.Width * 0.5f;
                //projectedB.Y += image.Height * 0.5f;
                int n = 2;
                Vector2F?[] pts = Util.RectIntersection(projectedA, projectedB, new Vector2F(0, 0), new Vector2F(image.Width, 0), new Vector2F(image.Width, image.Height), new Vector2F(0, image.Height), ref n);
                if(n == 0 && (projectedA.X >= image.Width || projectedA.X < 0 || projectedA.Y >= image.Height || projectedA.Y < 0 ||
                              projectedB.X >= image.Width || projectedB.X < 0 || projectedB.Y >= image.Height || projectedB.Y < 0))
                    continue;
                Vector2F la, lb;
                if(n > 0)
                {
                    List<int> nonNullIndices = new List<int>();
                    for(int i = 0; i < pts.Length; i++)
                    {
                        if(pts[i] != null)
                            nonNullIndices.Add(i);
                    }
                    if(n == 2)
                    {
                        la = pts[nonNullIndices[0]]!.Value;
                        lb = pts[nonNullIndices[1]]!.Value;
                    } else if(n == 1)
                    {
                        Vector2F pt = pts[nonNullIndices[0]]!.Value;
                        la = pt;
                        if(projectedA.X >= 0 && projectedA.X < image.Width && projectedA.Y >= 0 && projectedA.Y < image.Height)
                            lb = projectedA;
                        else
                            lb = projectedB;
                        /*if(pt.DistanceTo(projectedA) < pt.DistanceTo(projectedB))
                        {
                            la = pt;
                            lb = projectedB;
                        }
                        else
                        {
                            la = projectedA;
                            lb = pt;
                        }*/
                    }
                    else throw new Exception();
                } else
                {
                    la = projectedA;
                    lb = projectedB;
                }
                if(new float[] { la.X, la.Y, lb.X, lb.Y }.Any(float.IsNaN))
                    continue;
                Util.BresenhamLine(la, lb, (x, y) =>
                {
                    if(x >= 0 && y >= 0 && x < image.Width && y < image.Height)
                        image[x, y] = lineCol;
                });
            }
        }

        private bool Project(Vector3F p, out Vector2F outPt, out Vector3F pt3dproj)
        {
            Matrix m = new Matrix(4, 1);
            m[0, 0] = p.X;
            m[1, 0] = p.Y;
            m[2, 0] = p.Z;
            m[3, 0] = 1;

            m = Matrix.Multiply(m, _cam2world_translate);
            m = Matrix.Multiply(m, _cam2world_rot);
            pt3dproj = new Vector3F(m[0, 0], m[1, 0], m[2, 0]);
            //Console.WriteLine(m);
            m = Matrix.Multiply(m, _cam_clip_mat);
            //Console.WriteLine(m);
            for(int y = 0; y < m.Height; y++)
                for(int x = 0; x < m.Width; x++)
                {
                    float w = m[3, y];
                    m[x, y] /= w;
                    //if(m[x, y] > 1.001f || m [x, y] < -1.001f)
                    //if(MathF.Abs(m[x, y]) > FarClipPlane)
                    /*{
                        outPt = Vector2F.Zero;
                        return false;
                    }*/
                }
            m = Matrix.Multiply(m, _screen_mat);
            outPt = new Vector2F()
            {
                X = m[0, 0],
                Y = m[1, 0]
            };
            return true;

            /*Matrix m = new Matrix(1, 3);
            m[0, 0] = p.X - CameraPos.X;
            m[0, 1] = p.Y - CameraPos.Y;
            m[0, 2] = p.Z - CameraPos.Z;

            m = Matrix.Multiply(_matrixZRot, m);
            m = Matrix.Multiply(_matrixYRot, m);
            m = Matrix.Multiply(_matrixXRot, m);
            
            float z =  -CameraPos.Z -m[0, 2];
            bool valid = true;//m[0, 2] < 0;
            
            z = 1f / z; // TODO: Fix
            Matrix projectionMatrix = new Matrix(3, 2);
            projectionMatrix[0, 0] = z;
            projectionMatrix[1, 1] = z;
            m = Matrix.Multiply(projectionMatrix, m);

            vec = new Vector2F()
            {
                X = m[0, 0],
                Y = m[0, 1]
            };
            return valid;*/
        }
    }
}