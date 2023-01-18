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

        public float ClosestNeighborCount { get; set; } = 2;

        public Color Background { get; set; } = new Color(0, 0, 0);
        public Color LineColor { get; set; } = new Color(255, 0, 0);

        public Vector3F BoundingBoxMin { get; set; } = new Vector3F(-10, -10, -10);
        public Vector3F BoundingBoxMax { get; set; } = new Vector3F(10, 10, 10);
        
        private Vector3F[]? _vertices;
        private Vector3F[]? _verticesMapped;
        private Vector3F[]? _verticesDestination;
        private List<Tuple<int, int>> _edgeTable = new List<Tuple<int, int>>();
        private List<Tuple<int, int>> _edgeTableCurrent = new List<Tuple<int, int>>();
        private Random? _rnd;

        public Vector3F CameraPos { get; set; } = new Vector3F(0, 0, -15);
        private Vector3F _camRotCurrent;
        private Vector3F _camPosCurrent;
        public float _fovCurrent;
        public Vector3F CameraRotation { get; set; } = new Vector3F(0, 0, 0);

        public Bezier[] CameraPath { get; set; } = new Bezier[]
        {
            new Bezier(new Vector3F(1f, -1.66f, 2.66f), 
                       new Vector3F(0.33f, -2.33f, 0.33f), 
                       new Vector3F(1.66f, -1.33f, 0.66f), 
                       new Vector3F(0.66f, -1.33f, 1.66f))
        };

        private Vector3F Forward => new Vector3F()
        {
            X = MathF.Cos(_camRotCurrent.X)*MathF.Cos(_camRotCurrent.Y),
            Y = MathF.Sin(_camRotCurrent.X)*MathF.Cos(_camRotCurrent.Y),
            Z = MathF.Sin(_camRotCurrent.Y)
        };
        public float FieldOfView { get; set; } = 110f;
        public float NearClipPlane { get; set; } = 0.01f;
        public float FarClipPlane { get; set; } = 2000f;

        private Matrix _cam2world_translate, _cam2world_rot, _cam_clip_mat, _screen_mat;
        private float _verticalFieldOfView;

        public Generator()
        { }

        public void Setup(int seed = 133769420)
        {
            /*_edgeTable.Clear();
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
            _edgeTable.Add(new Edge() { A = _vertices[3], B = _vertices[7] });*/

            _edgeTable.Clear();
            _edgeTableCurrent.Clear();
            _rnd = new Random(seed);
            Func<float, float, float> gen = (min, max) => 
            {
                float g;
                while((g = _rnd.Gaussian()) < -1 || g > 1);
                return Util.Map(g, -1, 1, min, max);
            };
            _vertices = new Vector3F[PointCount];
            for(int i = 0; i < PointCount; i++)
            {
                _vertices[i].X = gen(BoundingBoxMin.X, BoundingBoxMax.X);
                _vertices[i].Y = gen(BoundingBoxMin.Y, BoundingBoxMax.Y);
                _vertices[i].Z = gen(BoundingBoxMin.Z, BoundingBoxMax.Z);
            }

            bool[] visited = new bool[PointCount];
            List<Tuple<Vector3F, int>> buffer = new List<Tuple<Vector3F, int>>();
            Tuple<Vector3F, int>? pointOfInterest = null;
            for(int i = 0; i < PointCount; i++)
            {
                pointOfInterest = Tuple.Create(_vertices[i], i);
                visited[i] = true;
                while(buffer.Count < ClosestNeighborCount)
                {
                    var min = _vertices.Select((x, k) => Tuple.Create(x, k))
                         .Where((x) => !visited[x.Item2])
                         .MinBy(x => x.Item1.DistanceTo(pointOfInterest.Item1));
                    if(min == null)
                        break;
                    visited[min.Item2] = true;
                    buffer.Add(min);
                }
                foreach(var tuple in buffer)
                {
                    visited[tuple.Item2] = false;
                    //if(_rnd.NextSingle() < ClosestNeighborConnectionProbability)
                    if(tuple.Item1.DistanceTo(pointOfInterest.Item1) < 50)
                    {
                        _edgeTable.Add(Tuple.Create(pointOfInterest.Item2, tuple.Item2));
                    }
                }
                buffer.Clear();
            }

            _verticesMapped = new Vector3F[_vertices.Length];
            _verticesDestination = new Vector3F[_vertices.Length];
            _edgeTableCurrent.AddRange(_edgeTable);
        }

        public bool[,]? Pattern { get; set; } = null;

        private List<Vector2F> _patternPts = new List<Vector2F>();
        private Vector2F _patternMass;

        public void PatternProcess()
        {
            if(Pattern == null)
                throw new InvalidOperationException("No pattern set");
            _patternPts.Clear();
            _patternMass = new Vector2F(0, 0);
            float w = Pattern.GetLength(0), h = Pattern.GetLength(1);
            for(int y = 0; y < h; y++)
            {
                for(int x = 0; x < w; x++)
                {
                    if(Pattern[x, y])
                    {
                        _patternMass += new Vector2F(x / w, y / h);
                        _patternPts.Add(new Vector2F(x / w, y / h));
                    }
                }
            }
            if(_patternPts.Count > 0)
            {
                float div = 1f / _patternPts.Count;
                _patternMass *= div;
                Vector2F pointOfInterest = _patternPts[0];
                _patternPts = _patternPts.OrderBy(x => x.DistanceTo(pointOfInterest)).ToList();
            }
        }

        public void LerpPattern(float percentage)
        {
            if(_verticesMapped == null || _vertices == null || _verticesDestination == null)
                    throw new InvalidOperationException("Pattern has not yet been processed.");
            if(Pattern == null)
                _verticesMapped = _verticesDestination = _vertices;
            else
            {
                if(percentage > 1)
                    percentage = 1;
                for(int k = 0; k < _vertices.Length; k++)
                    _verticesMapped[k] = Vector3F.Lerp(_vertices[k], _verticesDestination[k], percentage);
            }
        }

        private bool[] _calcPatternVisited;
        private bool[] _calcPatternVisitedDestGlobal;
        private Vector2F[] _calcPatternCondensedPts;

        private int[] FindEdges(int containingVertex)
        {
            return _edgeTable.Where(x => x.Item1 == containingVertex || x.Item2 == containingVertex)
                             .Select(x => x.Item1 == containingVertex ? x.Item2 : x.Item1).ToArray();
        }

        private void CalcPatternVertex(int which, Vector2F nearest)
        {
            if(_verticesDestination == null)
                throw new Exception();
            Matrix rMat = Matrix.RotationMatrix4x4(new Vector3F(0, 0, 0));

            
            nearest -= _patternMass;
            Vector3F vec = new Vector3F(nearest.X, nearest.Y * -1, 0);
            
            Matrix m = new Matrix(4, 1);
            m[0, 0] = vec.X;
            m[1, 0] = vec.Y;
            m[2, 0] = vec.Z;
            m[3, 0] = 1;
            m = Matrix.Multiply(m, rMat);
            vec = new Vector3F(m[0, 0], m[1, 0], m[2, 0]);
            
            vec *= 30f;
            vec += new Vector3F(0, 0, 0);
            _verticesDestination[which] = vec;
            //System.Console.WriteLine(vec);
        }

        private void CalcPatternBranchRecursive(int startingVertex, int patternPt)
        {
            if(_verticesDestination == null)
                throw new Exception();
            
            //const float maxDistance = 0.1f;

            int[] cur;
            Vector2F pointOfInterest = _calcPatternCondensedPts[patternPt];
            while(true)
            {
                cur = FindEdges(startingVertex);
                if(cur.Length == 0)
                    break;
                int processed = 0;
                bool firstVisited = false;
                int firstContinuationIdx = -1;
                for(int i = 0; i < cur.Length; i++)
                {
                    if(_calcPatternVisited[cur[i]])
                        continue;
                    if(firstContinuationIdx == -1)
                        firstContinuationIdx = i;
                    var f = _calcPatternCondensedPts.Select((x, i) => new { idx = i, val = x })
                                                    .Where(x => !_calcPatternVisitedDestGlobal[x.idx])
                                                    .MinBy(x => x.val.DistanceTo(pointOfInterest));
                    if(f == null)
                        continue;
                    if(i == firstContinuationIdx)
                    {
                        firstVisited = _calcPatternVisited[cur[firstContinuationIdx]];
                        patternPt = f.idx;
                        
                    }
                    
                    _calcPatternVisitedDestGlobal[f.idx] = true;
                    _calcPatternVisited[cur[i]] = true;
                    CalcPatternVertex(cur[i], f.val);
                    /*if(_verticesDestination[cur[i]].DistanceTo(_verticesDestination[startingVertex]) > maxDistance)
                    {
                        _verticesDestination[cur[i]] = _verticesDestination[startingVertex];
                    }*/
                    
                    if(i != firstContinuationIdx)
                        CalcPatternBranchRecursive(cur[i], f.idx);
                    processed++;
                }
                if(processed == 0 || firstVisited)
                    break;
                startingVertex = cur[firstContinuationIdx];
                pointOfInterest = _calcPatternCondensedPts[patternPt];
            }
        }

        public void CalcPattern()
        {
            if(Pattern == null || _patternPts == null || _verticesMapped == null || _vertices == null || _verticesDestination == null)
                    throw new InvalidOperationException("Pattern has not yet been processed.");
            if(_patternPts.Count == 0)
            {
                _verticesDestination = _vertices;
                return;
            }
            //Matrix inv_screen_mat = _screen_mat.Inverse4x4() ?? throw new Exception("Failed to invert matrix.");
            //Matrix inv_cam_clip_mat = _cam_clip_mat.Inverse4x4() ?? throw new Exception("Failed to invert matrix.");
            //Matrix inv_cam2world_rot = _cam2world_rot.Inverse4x4() ?? throw new Exception("Failed to invert matrix.");
            //Matrix inv_cam2world_translate = _cam2world_translate.Inverse4x4() ?? throw new Exception("Failed to invert matrix.");
            
            int[] idxs = new int[2];
            float hDelta = _patternPts.Count / (float)_vertices.Length;
            if(hDelta < 1)
                hDelta = 1;
            _calcPatternCondensedPts = new Vector2F[_vertices.Length];
            int j = 0;
            for(float f = 0; j < _calcPatternCondensedPts.Length; j++, f += hDelta)
            {
                _calcPatternCondensedPts[j] = _patternPts[(int)f];
            }

            _calcPatternVisited = new bool[_vertices.Length];
            _calcPatternVisitedDestGlobal = new bool[_calcPatternCondensedPts.Length];

            //Array.Copy(_vertices, _verticesDestination, _vertices.Length);
            for(int k = 0; k < _verticesDestination.Length; k++)
                _verticesDestination[k] = new Vector3F(float.NaN, float.NaN, float.NaN);

            const int kk = 0;
            _calcPatternVisitedDestGlobal[kk] = true;
            _calcPatternVisited[kk] = true;
            CalcPatternVertex(kk, _calcPatternCondensedPts[kk]);
            CalcPatternBranchRecursive(kk, kk);

            const float maxDistance = 1f;
            _edgeTableCurrent.Clear();
            _edgeTableCurrent.AddRange(_edgeTable);
            for(int w = 0; w < _edgeTable.Count; w++)
            {
                var t = _edgeTable[w];
                if(_verticesDestination[t.Item1].DistanceTo(_verticesDestination[t.Item2]) > maxDistance)
                {
                    _edgeTableCurrent[w] = Tuple.Create(_edgeTableCurrent[w].Item1, _edgeTableCurrent[w].Item1);
                }
            }
            
            /*bool[] visited = new bool[_vertices.Length];
            bool[] visited2 = new bool[_vertices.Length];
            Vector2F pointOfInterest = new Vector2F();
            for(int kk = 0; kk < _edgeTable.Count; kk++)
            {
                idxs[0] = _edgeTable[kk].Item1;
                idxs[1] = _edgeTable[kk].Item2;
                bool first = true;
                bool breakFully = false;
                if(idxs.All(x => !visited[x]))
                    foreach(int k in idxs)
                    {
                        //if(visited[k]) continue;
                        visited[k] = true;
                        Vector2F nearest;
                        if(first)
                        {
                            var f = visited2.Select((x, i) => new { idx = i, val = x }).Where(x => !x.val).FirstOrDefault()?.idx;
                            if(f == null)
                            {
                                breakFully = true;
                                break;
                            }
                            pointOfInterest = condensedPts[f.Value];
                            visited2[f.Value] = true;
                            nearest = pointOfInterest;
                            first = false;
                        } else
                        {
                            var nearestN = condensedPts.Select((x, i) => new { idx = i, val = x })
                                                .Where(x => !visited2[x.idx])
                                                .MinBy(x => x.val.DistanceTo(pointOfInterest));
                            if(nearestN == null)
                            {
                                breakFully = true;
                                break;
                            }
                            visited2[nearestN.idx] = true;
                            nearest = nearestN.val;
                            pointOfInterest = nearest;
                        }
                        System.Console.WriteLine(nearest);
                        nearest -= _patternMass;
                        Vector3F vec = new Vector3F(nearest.X, nearest.Y * -1, 0);
                        
                        Matrix m = new Matrix(4, 1);
                        m[0, 0] = vec.X;
                        m[1, 0] = vec.Y;
                        m[2, 0] = vec.Z;
                        m[3, 0] = 1;
                        m = Matrix.Multiply(m, rMat);
                        vec = new Vector3F(m[0, 0], m[1, 0], m[2, 0]);
                        
                        vec *= 30f;
                        vec += new Vector3F(0, 0, 0);
                        /*Vector2F proj2d;
                        Vector3F proj;
                        if(Project(vec, out proj2d, out proj))
                        {
                            //Vector2F nearest = _patternPts.MinBy(x => x.DistanceTo(proj2d));
                            Vector2F nearest = _patternPts[h];
                            Matrix m = new Matrix(4, 1);
                            m[0, 0] = nearest.X;
                            m[1, 0] = nearest.Y;
                            m[2, 0] = 0.98f;
                            m[3, 0] = 1;
                            //System.Console.WriteLine();
                            //System.Console.WriteLine();
                            //System.Console.WriteLine(m);
                            m = Matrix.Multiply(m, inv_screen_mat);
                            //System.Console.WriteLine(m);
                            //System.Console.WriteLine(m);
                            m = Matrix.Multiply(m, inv_cam_clip_mat);
                            //System.Console.WriteLine(m);
                            m = Matrix.Multiply(m, inv_cam2world_rot);
                            //System.Console.WriteLine(m);
                            m = Matrix.Multiply(m, inv_cam2world_translate);
                            /*System.Console.WriteLine(m);
                            for(int y = 0; y < m.Height; y++)
                                for(int x = 0; x < m.Width; x++)
                                {
                                    float w = m[3, y];
                                    m[x, y] /= w;
                                }
                            System.Console.WriteLine(m);* /
                            vec = new Vector3F(m[0, 0], m[1, 0], m[2, 0]);
                        }* /
                        _verticesDestination[k] = vec;
                    }
                if(breakFully)
                    break;
            }*/
        }

        public void SetMatrices(float time, int imageWidth, int imageHeight)
        {
            if(_vertices == null || _verticesMapped == null || _verticesDestination == null)
                throw new InvalidOperationException("Generator.Setup has not yet been called.");
            _camRotCurrent = CameraRotation;
            _camPosCurrent = CameraPos;
            _fovCurrent = FieldOfView;
            int i = (int)(time * CameraPath.Length);
            float bezierTime = (time - i) * (float)CameraPath.Length;
            _camPosCurrent += CameraPath[i % CameraPath.Length].Point(bezierTime);
            //Console.WriteLine(_camPosCurrent);
            //camRot.X = MathF.Sin(time * MathF.PI) * 180f;
            //_camRotCurrent.Z += (time * MathF.PI * 2f) * Util.Rad2Deg * 0.13f;
            //Console.WriteLine(_camRotCurrent.Y);
            //camRot.Z = MathF.Sin(time * MathF.PI * 3.33f) * 180f;
            /*float cConst = time * MathF.PI * 2f;
            float cSin = MathF.Sin(cConst);
            float cCos = MathF.Cos(cConst);
            float fSin = MathF.Abs(cSin);

            const float rad = 9f;

            _camPosCurrent.X = cCos * rad;
            _camPosCurrent.Z = cSin * rad;
            //_camPosCurrent.Y = Util.Lerp(1500, -1500, time);

            //_camRotCurrent.X += (time * MathF.PI * 2f) * Util.Rad2Deg;
            _camRotCurrent.Y += (-cConst + MathF.PI * 1.5f) * Util.Rad2Deg;*/
            //_camRotCurrent.Z = (-cConst + MathF.PI * 1.5f) * 90f;
            _camRotCurrent.Z = MathF.Sin(time * MathF.PI * 2f) * 5f;

            //float lAngle = MathF.Atan2(BoundingBoxMax.Y - BoundingBoxMin.Y, rad) * Util.Rad2Deg;
            
            //_camRotCurrent.X = MathF.Pow(cSin, 4) * lAngle;
            //_camRotCurrent.X = 90f;//MathF.Pow(cSin, 4) * lAngle;
            //Console.WriteLine($"{_camRotCurrent.X} - {lAngle}");
            //_camRotCurrent.Z = MathF.PI * 0.5f + time * MathF.PI * 2f * Util.Rad2Deg;

            
            //fSin = fSin * fSin * fSin;*/

            _fovCurrent = _fovCurrent - MathF.Pow(MathF.Sin(time * MathF.PI * 2f), 4) * 6f;

            _verticalFieldOfView = _fovCurrent * (imageHeight / (float)imageWidth);
            
            _cam2world_translate = new Matrix(4, 4);
            _cam2world_translate[0, 0] = 1;
            _cam2world_translate[1, 1] = 1;
            _cam2world_translate[3, 1] = 1;
            _cam2world_translate[2, 2] = 1;
            _cam2world_translate[3, 3] = 1;
            _cam2world_translate[0, 3] = -_camPosCurrent.X;
            _cam2world_translate[1, 3] = -_camPosCurrent.Y;
            _cam2world_translate[2, 3] = -_camPosCurrent.Z;
            
            _cam2world_rot = Matrix.RotationMatrix4x4(_camRotCurrent);

            _cam_clip_mat = new Matrix(4, 4);
            float right = MathF.Tan(Util.Deg2Rad * _fovCurrent * 0.5f), left = -right, top = MathF.Tan(Util.Deg2Rad * _verticalFieldOfView * 0.5f), bottom = -top;
            _cam_clip_mat[0, 0] = 2f / (right - left);
            _cam_clip_mat[1, 1] = 2f / (top - bottom);
            _cam_clip_mat[2, 2] = (FarClipPlane + NearClipPlane) / (FarClipPlane - NearClipPlane);
            _cam_clip_mat[2, 3] = -2 * NearClipPlane * FarClipPlane / (FarClipPlane - NearClipPlane);
            _cam_clip_mat[3, 2] = 1;

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
            if(_verticesMapped == null)
                throw new InvalidOperationException("Matrices not initialized!");
            Rgba32 lineCol = new Rgba32(LineColor.R, LineColor.G, LineColor.B);
            //Rgba32 lineCol2 = new Rgba32(LineColor.R, LineColor.G, LineColor.B);
            image.Mutate(x => x.Fill(new Rgba32(Background.R, Background.G, Background.B)));
            Vector3F one = new Vector3F(0, 0, 1);
            List<Tuple<Vector2F, Vector2F>> ptsl = new List<Tuple<Vector2F, Vector2F>>();
            //System.Console.WriteLine(string.Join(", ", _edgeTable));
            foreach(Edge e in _edgeTableCurrent.Select(x => new Edge(_verticesMapped[x.Item1], _verticesMapped[x.Item2]))/*.Take(150)*/)
            {
                Vector2F projectedA, projectedB;
                Vector3F pA3d, pB3d;
                
                bool va = Project(e.A, out projectedA, out pA3d);
                bool vb = Project(e.B, out projectedB, out pB3d);

                float d1 = Vector3F.Dot(one, pA3d);
                float d2 = Vector3F.Dot(one, pB3d);
                
                if(d1 < 0 || d2 < 0 || !va || !vb) continue;
                //projectedA = projectedB;
                Vector2F la, lb;
                int n = 2;
                Vector2F?[] pts = Util.RectIntersection(projectedA, projectedB, new Vector2F(0, 0), new Vector2F(image.Width, 0), new Vector2F(image.Width, image.Height), new Vector2F(0, image.Height), ref n);
                if(n == 0 && (projectedA.X >= image.Width || projectedA.X < 0 || projectedA.Y >= image.Height || projectedA.Y < 0 ||
                              projectedB.X >= image.Width || projectedB.X < 0 || projectedB.Y >= image.Height || projectedB.Y < 0))
                    continue;
                
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
                    }
                    else throw new Exception();
                } else
                {
                    la = projectedA;
                    lb = projectedB;
                }
                if(new float[] { la.X, la.Y, lb.X, lb.Y }.Any(float.IsNaN))
                    continue;
                ptsl.Add(Tuple.Create(la, lb));
                Util.BresenhamLine(la, lb, (x, y) =>
                {
                    if(x >= 0 && y >= 0 && x < image.Width && y < image.Height)
                        image[x, y] = lineCol;
                });
            }
            /*foreach(var tpl in ptsl)
            {
                int x = (int)tpl.Item1.X, y = (int)tpl.Item1.Y;
                if(x >= 0 && y >= 0 && x < image.Width && y < image.Height)
                    image[x, y] = lineCol;
                
                x = (int)tpl.Item2.X; y = (int)tpl.Item2.Y;
                if(x >= 0 && y >= 0 && x < image.Width && y < image.Height)
                    image[x, y] = lineCol;
            }*/
        }

        private bool Project(Vector3F p, out Vector2F outPt, out Vector3F pt3dproj)
        {
            Matrix m = new Matrix(4, 1);
            m[0, 0] = p.X;
            m[1, 0] = p.Y;
            m[2, 0] = p.Z;
            m[3, 0] = 1;

            //System.Console.WriteLine(m);
            m = Matrix.Multiply(m, _cam2world_translate);
            //System.Console.WriteLine(m);
            m = Matrix.Multiply(m, _cam2world_rot);
            //System.Console.WriteLine(m);
            pt3dproj = new Vector3F(m[0, 0], m[1, 0], m[2, 0]);
            m = Matrix.Multiply(m, _cam_clip_mat);
            //System.Console.WriteLine(m);
            for(int y = 0; y < m.Height; y++)
                for(int x = 0; x < m.Width; x++)
                {
                    float w = m[3, y];
                    m[x, y] /= w;
                }
            //System.Console.WriteLine(m);
            m = Matrix.Multiply(m, _screen_mat);
            //System.Console.WriteLine(m);
            outPt = new Vector2F()
            {
                X = m[0, 0],
                Y = m[1, 0]
            };
            return true;
        }
    }
}