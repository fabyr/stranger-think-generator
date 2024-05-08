using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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

            const int w = 1280, h = 720;

            bool[,] pattern = new bool[w, h];
            using(Image<Rgba32> image = (Image<Rgba32>)Image<Rgba32>.Load("pattern9.png"))
            {
                image.Mutate(x => x.Resize(w, h));
                for(int y = 0; y < h; y++)
                {
                    for(int x = 0; x < w; x++)
                    {
                        Rgba32 c = image[x, y];
                        pattern[x, y] = (c.R + c.G + c.B) / 3f > 128f;
                    }
                }
            }

            Generator gen = new Generator();
            gen.Pattern = pattern;
            gen.Setup();
            gen.PatternProcess();
            
            const float tzero = 0.8f;
            const float stiffness = 2;
            using(Image<Rgba32> image = new Image<Rgba32>(w, h))
            {
                const int images = 230;
                const int fps = 200;
                for(int i = 0; i < images; i++)
                {
                    System.Console.WriteLine($"#{i}");
                    float time = i * (1f / fps);
                    gen.SetMatrices(time, image.Width, image.Height);
                    if(i == 0)
                        gen.CalcPattern();
                    float ltime = time >= tzero ? 1 : -MathF.Pow((time - tzero - (1/stiffness)) * stiffness, -3);
                    //System.Console.WriteLine(ltime);
                    gen.LerpPattern(ltime, ltime);
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