using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.IO;

namespace StrangerThinkGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const int w = 1280, h = 720;
            const string outputDir = "frames";

            bool[,] pattern = new bool[w, h];
            using (Image<Rgba32> image = Image.Load("pattern10.png").CloneAs<Rgba32>())
            {
                image.Mutate(x => x.Resize(w, h));
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        Rgba32 c = image[x, y];
                        pattern[x, y] = (c.R + c.G + c.B) / 3f > 128f;
                    }
                }
            }
            Directory.CreateDirectory(outputDir);

            Generator gen = new()
            {
                Pattern = pattern
            };
            gen.Setup();
            gen.PatternProcess();

            const float tzero = 0.8f;
            const float stiffness = 2.0f;
            using (Image<Rgba32> image = new(w, h))
            {
                const int images = 120;
                const int fps = 100;
                for (int i = 0; i < images; i++)
                {
                    Console.WriteLine($"Rendering Frame #{i + 1} of {images}");
                    float time = i * (1f / fps);
                    gen.SetMatrices(time, image.Width, image.Height);
                    if (i == 0)
                        gen.CalcPattern();
                    float ltime = time >= tzero ? 1 : -MathF.Pow((time - tzero - (1 / stiffness)) * stiffness, -3);
                    gen.LerpPattern(ltime, ltime);
                    gen.RenderFrame(image);
                    image.SaveAsPng(Path.Join(outputDir, $"{i}.png"));
                }
            }
            ProcessStartInfo psi = new()
            {
                Arguments = "-f image2 -i %d.png ../output.gif -y",
                WorkingDirectory = "frames",
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            Process p = new()
            {
                EnableRaisingEvents = true,
                StartInfo = psi
            };
            p.OutputDataReceived += (s, e) => Console.Write(e.Data);
            p.ErrorDataReceived += (s, e) => Console.Error.Write(e.Data);
            if (p.Start())
            {
                p.WaitForExit();
                Directory.Delete(outputDir, true);
            }
            else
                Console.WriteLine("Failed to start ffmpeg. You need to manually convert all frames to a video/gif. (In the 'frames' directory.)");
            Console.WriteLine("Done");
        }
    }
}