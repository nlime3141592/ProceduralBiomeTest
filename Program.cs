using BigGustave;
using System;
using System.IO;

namespace nl
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Main_DiamondSquare(args);
            // Main_CubicConvolution(args);
            Main_Materialization(args);
        }

        private static void Main_DiamondSquare(string[] args)
        {
            DiamondSquare ds = new DiamondSquare(
                resolution: 8,
                gaussScale: 0.1f,
                h: 0.9f,
                252.0f, 62.0f, 133.0f,
                9.0f, 107.0f, 239.0f
            );

            ds.Create();
            ds.Save();
        }

        private static void Main_CubicConvolution(string[] args)
        {
            FileStream fs = new FileStream("C:\\Test\\2025-12-12-20-50-59.png", FileMode.Open, FileAccess.Read);
            Png png = PngOpener.Open(fs);
            fs.Close();

            int tw = 100;
            int th = 100;

            float[,] origin = new float[png.Width, png.Height];

            for (int x = 0; x < png.Width; ++x)
            {
                for (int y = 0; y < png.Height; ++y)
                {
                    origin[x, y] = (float)(png.GetPixel(x, y).R);
                }
            }

            float[,] target = CubicConvolution.Scaling(origin, png.Width, png.Height, tw, th);
            PngBuilder builder = PngBuilder.Create(tw, th, false);

            for (int x = 0; x < tw; ++x)
            {
                for (int y = 0; y < th; ++y)
                {
                    float v = target[x, y];
                    Pixel px = new Pixel((byte)v, (byte)v, (byte)v);
                    builder.SetPixel(px, x, y);
                }
            }

            fs = new FileStream("C:\\Test\\2025-12-12-20-50-59 (2).png", FileMode.Create);
            builder.Save(fs);
            fs.Close();
        }

        private static void Main_Materialization(string[] args)
        {
            string pathOut = "C:\\Test\\tile-test-1 (1-1).png";
            string pathSrc = "C:\\Test\\tile-test-1.png";
            string pathNzs = "C:\\Test\\2025-12-12-20-50-59 (1).png";

            Materialization.Synthesize(
                pathOut, pathSrc, pathNzs,
                0.2f, 1.0f,
                255.0f, 255.0f, 255.0f
            );
        }
    }
}