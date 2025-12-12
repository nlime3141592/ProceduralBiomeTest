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
            // Main_Materialization(args);
            Main_Biome1(args);
        }

        private static void Main_DiamondSquare(string[] args)
        {
            for (int i = 0; i < 100; ++i)
            {
                string path = @$"C:\Test\DiamondSquare\noise-{string.Format("{0:D08}", i)}.png";
                DiamondSquare ds = new DiamondSquare(
                    resolution: 8,
                    gaussScale: 0.1f,
                    h: 0.9f,
                    252.0f, 62.0f, 133.0f,
                    9.0f, 107.0f, 239.0f
                );

                ds.Create();
                ds.Save(path);
            }
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

        private static void Main_Biome1(string[] args)
        {
            for (int i = 0; i < 10; ++i)
            {
                Console.Write($"biome ${i + 1} ... ");

                string number = string.Format("{0:D08}", i);
                string directory = @"C:\Test\DiamondSquareBiome";

                string pathAltitude = @$"{directory}\{number}-map-altitude.png";
                string pathTemperature = @$"{directory}\{number}-map-temperature.png";
                string pathHumidity = @$"{directory}\{number}-map-humidity.png";
                string pathBiome = @$"{directory}\{number}-biome.png";

                // 고도 경계
                float a0 = 0.35f;
                float a1 = 0.48f;
                float a2 = 0.65f;

                // 온도 경계
                float t0 = 0.35f;
                float t1 = 0.48f;
                float t2 = 0.65f;

                // 습도 경계
                float h0 = 0.35f;
                float h1 = 0.48f;
                float h2 = 0.65f;

                // Diamond Square 알고리즘의 파라미터
                int resolution = 10; // 실제 이미지 크기 == 2^(resolution) + 1
                float gaussScale = 0.1f;
                float h = 0.9f;

                // 고도 맵 생성
                DiamondSquare dsAltitude = new DiamondSquare(
                    resolution,
                    gaussScale, h,
                    0.0f, 0.0f, 0.0f,
                    1.0f, 1.0f, 1.0f);

                dsAltitude.Create();
                dsAltitude.Save(pathAltitude);

                // 온도 맵 생성
                DiamondSquare dsTemperature = new DiamondSquare(
                    resolution,
                    gaussScale, h,
                    0.0f, 0.0f, 0.0f,
                    1.0f, 1.0f, 1.0f);

                dsTemperature.Create();
                dsTemperature.Save(pathTemperature);

                // 습도 맵 생성
                DiamondSquare dsHumidity = new DiamondSquare(
                    resolution,
                    gaussScale, h,
                    0.0f, 0.0f, 0.0f,
                    1.0f, 1.0f, 1.0f);

                dsHumidity.Create();
                dsHumidity.Save(pathHumidity);

                // 이미지 읽기
                Png pngAltitude = null;
                Png pngTemperature = null;
                Png pngHumidity = null;

                using (FileStream fs = new FileStream(pathAltitude, FileMode.Open, FileAccess.Read))
                {
                    pngAltitude = PngOpener.Open(fs);
                }

                using (FileStream fs = new FileStream(pathTemperature, FileMode.Open, FileAccess.Read))
                {
                    pngTemperature = PngOpener.Open(fs);
                }

                using (FileStream fs = new FileStream(pathHumidity, FileMode.Open, FileAccess.Read))
                {
                    pngHumidity = PngOpener.Open(fs);
                }

                // 바이옴 생성 시작
                PngBuilder biomeBuilder = PngBuilder.Create(pngAltitude.Width, pngAltitude.Height, true);

                for (int x = 0; x < pngAltitude.Width; ++x)
                {
                    for (int y = 0; y < pngAltitude.Height; ++y)
                    {
                        // 지형 속성 식별
                        float va = (float)pngAltitude.GetPixel(x, y).R / 255.0f;
                        float vt = (float)pngTemperature.GetPixel(x, y).R / 255.0f;
                        float vh = (float)pngHumidity.GetPixel(x, y).R / 255.0f;

                        int sa = 3;
                        int st = 3;
                        int sh = 3;

                        if (va < a0) sa = 0;
                        else if (va < a1) sa = 1;
                        else if (va < a2) sa = 2;

                        if (vt < t0) st = 0;
                        else if (vt < t1) st = 1;
                        else if (vt < t2) st = 2;

                        if (vh < h0) sh = 0;
                        else if (vh < h1) sh = 1;
                        else if (vh < h2) sh = 2;

                        // 생물군계 결정
                        if (sa >= 2 && st >= 3)
                        {
                            // 용암
                            biomeBuilder.SetPixel(new Pixel(255, 127, 39, 255, false), x, y);
                            continue;
                        }
                        if (sa <= 1 && st <= 0)
                        {
                            // 얼음
                            biomeBuilder.SetPixel(new Pixel(153, 217, 234, 255, false), x, y);
                            continue;
                        }
                        if (sa == 1 && st == 3 && sh <= 1)
                        {
                            // 사막
                            biomeBuilder.SetPixel(new Pixel(239, 228, 176, 255, false), x, y);
                            continue;
                        }
                        if (sa >= 1 && (st == 1 || st == 2) && sh >= 2)
                        {
                            // 숲
                            biomeBuilder.SetPixel(new Pixel(34, 177, 76, 255, false), x, y);
                            continue;
                        }

                        // 조건문을 통과하지 못한 경우, 평지 처리 (임시 구조)
                        biomeBuilder.SetPixel(new Pixel(185, 122, 87, 255, false), x, y);
                        continue;
                    }
                }

                using (FileStream fs = new FileStream(pathBiome, FileMode.Create))
                {
                    biomeBuilder.Save(fs);
                }

                Console.WriteLine($"OK");
            }

            Console.WriteLine("All biome generated.");
        }
    }
}