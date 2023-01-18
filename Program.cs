using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;

namespace StrangerThinkGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*int[] d = new int[100];
            Random rnd = new Random();
            const float pow = 3;
            for(int i = 0; i < 1000000; i++)
            {
                float g;
                while((g = rnd.Gaussian()) < -1 || g > 1);
                float f = Util.Map(MathF.Pow(MathF.Abs(g), pow) * MathF.Sign(g), -1, 1, 0, 100);
                d[(int)f]++;
            }
            Console.WriteLine(string.Join(", ", d));
            return;*/

            const int w = 600, h = 600;

            bool[,] pattern = new bool[w, h];
            using(Image<Rgba32> image = (Image<Rgba32>)Image<Rgba32>.Load("pattern.png"))
            {
                for(int y = 0; y < h; y++)
                {
                    for(int x = 0; x < w; x++)
                    {
                        Rgba32 c = image[x, y];
                        pattern[x, y] = c.A > 128f;
                    }
                }
            }

            Generator gen = new Generator();
            gen.Pattern = pattern;
            gen.Setup();
            gen.PatternProcess();
            using(Image<Rgba32> image = new Image<Rgba32>(w, h))
            {
                const int images = 120;
                const int fps = 120;
                for(int i = 0; i < images; i++)
                {
                    System.Console.WriteLine($"#{i}");
                    float time = i * (1f / fps);
                    gen.SetMatrices(time, image.Width, image.Height);
                    if(i == 0)
                        gen.CalcPattern();
                    gen.LerpPattern((MathF.Pow(time, 3f)) * 2f);
                    gen.RenderFrame(image);
                    //System.Console.WriteLine($"{i} - {gen._fovCurrent}");
                    image.SaveAsPng($"anim/{i}.png");
                }
            }
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                Arguments = "-f image2 -i %d.png gif.gif -y",
                WorkingDirectory = "anim",
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process p = new Process();
            p.EnableRaisingEvents = true;
            p.OutputDataReceived += (s, e) => Console.Write(e.Data);
            p.ErrorDataReceived += (s, e) => Console.Error.Write(e.Data);
            p.StartInfo = psi;
            if(p.Start())
                p.WaitForExit();
            else
                Console.WriteLine("Failed to start ffmpeg.");
            Console.WriteLine("Done");
        }
    }
}