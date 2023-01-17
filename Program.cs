using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StrangerThinkGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Generator gen = new Generator();
            gen.Setup();
            using(Image<Rgba32> image = new Image<Rgba32>(600, 600))
            {
                const int images = 120;
                const int fps = 120;
                for(int i = 0; i < images; i++)
                {
                    gen.SetMatrices(i * (1f / fps), image.Width, image.Height);
                    gen.RenderFrame(image);
                    image.SaveAsPng($"anim/{i}.png");
                }
            }
            Console.WriteLine("Done");
        }
    }
}