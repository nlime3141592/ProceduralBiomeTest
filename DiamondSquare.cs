using BigGustave;
using System;
using System.Diagnostics;
using System.IO;

namespace nl
{
    public class DiamondSquare
    {
        private int _resolution;
        private int _size;
        private float[,] _map;

        // color 0
        private float r0;
        private float g0;
        private float b0;

        // color 1
        private float r1;
        private float g1;
        private float b1;

        private Random _prng;
        private float _gaussScale;

        public DiamondSquare(
            int resolution,
            float gaussScale,
            float r0, float g0, float b0,
            float r1, float g1, float b1)
        {
            Debug.Assert(resolution > 0 && resolution < 12, "Invalid image size.");

            _resolution = resolution;
            _size = 1 << (_resolution + 1) + 1;

            _map = new float[_size, _size];

            this.r0 = r0;
            this.g0 = g0;
            this.b0 = b0;

            this.r1 = r1;
            this.g1 = g1;
            this.b1 = b1;

            _prng = new Random();
            _gaussScale = gaussScale;
        }

        public void Create()
        {
            int s = _size - 1;

            _map[0,0] = _prng.NextSingle();
            _map[0,s] = _prng.NextSingle();
            _map[s,0] = _prng.NextSingle();
            _map[s,s] = _prng.NextSingle();

            Create_Rec(0, 0, s, s, 1);
        }

        private void Create_Rec(int x0, int y0, int x1, int y1, float depth)
        {
            if (x0 + 1 == x1)
                return;

            int xm = (x0 + x1) / 2;
            int ym = (y0 + y1) / 2;

            CreateDiamond(x0, y0, x1, y1, xm, ym, depth);
            CreateSquare(x0, y0, x1, y1, xm, ym, depth);

            float depth2 = depth * 0.5f;

            Create_Rec(x0, y0, xm, ym, depth2);
            Create_Rec(xm, y0, x1, ym, depth2);
            Create_Rec(x0, ym, xm, y1, depth2);
            Create_Rec(xm, ym, x1, y1, depth2);
        }

        private void CreateDiamond(int x0, int y0, int x1, int y1, int xm, int ym, float depth)
        {
            _map[xm, ym] = GetAvg4(
                _map[x0, y0],
                _map[x1, y0],
                _map[x0, y1],
                _map[x1, y1],
                depth
            );
        }

        private void CreateSquare(int x0, int y0, int x1, int y1, int xm, int ym, float depth)
        {
            _map[x0, ym] = GetAvg3(
                _map[x0, y0],
                _map[xm, ym],
                _map[x0, y1],
                depth
            );

            _map[xm, y0] = GetAvg3(
                _map[x0, y0],
                _map[xm, ym],
                _map[x1, y0],
                depth
            );

            _map[x1, ym] = GetAvg3(
                _map[x1, y0],
                _map[xm, ym],
                _map[x1, y1],
                depth
            );

            _map[xm, y1] = GetAvg3(
                _map[x0, y1],
                _map[xm, ym],
                _map[x1, y1],
                depth
            );
        }

        private float GetAvg3(float v0, float v1, float v2, float depth)
        {
            float avg = (v0 + v1 + v2) / 3.0f;

            return avg + GaussRandom(depth);
        }

        private float GetAvg4(float v0, float v1, float v2, float v3, float depth)
        {
            float avg = (v0 + v1 + v2 + v3) / 4.0f;

            return avg + GaussRandom(depth);
        }

        private float GaussRandom(float depth)
        {
            float u1 = 1.0f - _prng.NextSingle();
            float u2 = 1.0f - _prng.NextSingle();

            float randStdNormal = MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Sin(2.0f * MathF.PI * u2);

            return randStdNormal * _gaussScale * depth;
        }

        public string Save()
        {
            DateTime time = DateTime.Now;
            int YYYY = time.Year;
            int MM = time.Month;
            int DD = time.Day;
            int hh = time.Hour;
            int mm = time.Minute;
            int ss = time.Second;

            string dateString = string.Format("{0}-{1:D02}-{2:D02}-{3:D02}-{4:D02}-{5:D02}", YYYY, MM, DD, hh, mm, ss);
            string path = $@"C:\Test\{dateString}.png";

            PngBuilder builder = PngBuilder.Create(_size, _size, false);

            for (int x = 0; x < _size; ++x)
            {
                for (int y = 0; y < _size; ++y)
                {
                    builder.SetPixel(GetPixel(x, y), x, y);
                }
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                builder.Save(fs);
            }

            return path;
        }

        private Pixel GetPixel(int x, int y)
        {
            // float r = r0 + (r1 - r0) * _map[x, y];
            // float g = g0 + (g1 - g0) * _map[x, y];
            // float b = b0 + (b1 - b0) * _map[x, y];

            float r = r0 + (r1 - r0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y]));
            float g = g0 + (g1 - g0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y]));
            float b = b0 + (b1 - b0) * MathF.Max(0.0f, MathF.Min(1.0f, _map[x, y]));

            return new Pixel((byte)r, (byte)g, (byte)b);
        }
    }
}